using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.Build.Profile;

namespace Wireframe
{
    /// <summary>
    /// Turns a <see cref="BuildUploaderGraph"/> into a runnable <see cref="UploadTaskV2"/>. Never reimplements
    /// build/copy logic - it only instantiates <see cref="BuildProfileSource"/>/<see cref="LocalPathDestination"/>/
    /// <see cref="DebugLogAction"/> from the graph's nodes and composes them into the <see cref="AUploadNodeV2"/>
    /// tree that <see cref="UploadTaskV2"/> executes.
    ///
    /// The graph can contain any number of source/destination/action nodes, wired directly one after another
    /// (sequential by default, since each node's "Execution" output only fires after it completes) or fanned out
    /// through a <see cref="SequentialGroupNode"/>/<see cref="ParallelGroupNode"/> for explicit sequencing/parallelism.
    ///
    /// Any data input port (BuildProfile, Clean Build, Local Path, Message, ...) can also be wired to a
    /// Blackboard variable or a constant node instead of using its own literal value - see
    /// <see cref="GetInputPortValue{T}"/>/<see cref="TryGetConnectedConstantValue{T}"/>.
    /// </summary>
    public static class BuildUploaderGraphCompiler
    {
        /// <summary>
        /// Cheap structural validation used by <see cref="BuildUploaderGraph.OnGraphChanged"/>.
        /// Does not instantiate any BuildProfileSource/LocalPathDestination/DebugLogAction objects.
        /// </summary>
        public static bool Validate(BuildUploaderGraph graph, GraphLogger infos)
        {
            List<StartNode> startNodes = graph.GetNodes().OfType<StartNode>().ToList();
            if (startNodes.Count == 0)
            {
                infos.LogError("Add a StartNode to the graph.", graph);
                return false;
            }

            foreach (StartNode extra in startNodes.Skip(1))
            {
                infos.LogWarning("A graph only supports one StartNode. Only the first one will be used.", extra);
            }

            if (GetNextNode(startNodes[0]) == null)
            {
                infos.LogWarning("StartNode isn't connected to anything - the graph won't do anything.", startNodes[0]);
            }

            return true;
        }

        /// <summary>
        /// Tracks everything gathered while walking the graph so it can be handed to a fresh
        /// <see cref="UploadTaskV2"/> once the whole tree has been compiled.
        /// </summary>
        private class CompileState
        {
            public readonly UploadTaskV2 Task;
            public readonly List<AUploadSource> Sources = new List<AUploadSource>();
            public readonly List<AUploadDestination> Destinations = new List<AUploadDestination>();
            public readonly List<AUploadAction> Actions = new List<AUploadAction>();

            // Lazily resolved output values, keyed by the node+port that produced them, so a downstream
            // node's input can be wired to any upstream node's output regardless of compile order or
            // which branch of a Sequence/Parallel group either one lives in.
            private readonly Dictionary<(INode, string), Func<string>> m_outputProviders = new Dictionary<(INode, string), Func<string>>();

            public CompileState(UploadTaskV2 task)
            {
                Task = task;
            }

            public void RegisterOutput(INode node, string portName, Func<string> provider)
            {
                m_outputProviders[(node, portName)] = provider;
            }

            public Func<string> GetOutputProvider(INode node, string portName)
            {
                return m_outputProviders.TryGetValue((node, portName), out Func<string> provider) ? provider : null;
            }
        }

        /// <summary>
        /// Compiles the graph into a runnable <see cref="UploadTaskV2"/>. Instantiates real
        /// <see cref="BuildProfileSource"/>/<see cref="LocalPathDestination"/>/<see cref="DebugLogAction"/> objects.
        /// </summary>
        public static bool TryCompile(BuildUploaderGraph graph, out UploadTaskV2 task, GraphLogger infos = null)
        {
            task = null;

            List<StartNode> startNodes = graph.GetNodes().OfType<StartNode>().ToList();
            if (startNodes.Count == 0)
            {
                infos?.LogError("Add a StartNode to the graph.", graph);
                return false;
            }

            INode firstNode = GetNextNode(startNodes[0]);
            if (firstNode == null)
            {
                infos?.LogError("StartNode isn't connected to anything.", startNodes[0]);
                return false;
            }

            // Created before the tree is compiled (with an empty root for now) so SetVariableNode can register
            // a Context command on newTask.Context while still walking the graph - see CompileSetVariable.
            UploadTaskV2 newTask = new UploadTaskV2(GraphDatabase.GetGraphAssetPath(graph));
            CompileState state = new CompileState(newTask);

            AUploadNodeV2 root = CompileChain(firstNode, infos, state);
            if (root == null)
            {
                return false;
            }

            newTask.SetRoot(root);
            foreach (AUploadSource source in state.Sources) newTask.RegisterLeaf(source);
            foreach (AUploadDestination destination in state.Destinations) newTask.RegisterLeaf(destination);
            foreach (AUploadAction action in state.Actions) newTask.RegisterLeaf(action);

            task = newTask;
            return true;
        }

        /// <summary>
        /// Compiles a straight-line run of nodes starting at <paramref name="startNode"/>, following the shared
        /// "Execution" port until it runs out. Multiple nodes are wrapped in a <see cref="SequenceNodeV2"/>.
        /// </summary>
        private static AUploadNodeV2 CompileChain(INode startNode, GraphLogger infos, CompileState state)
        {
            List<AUploadNodeV2> steps = new List<AUploadNodeV2>();

            INode node = startNode;
            while (node != null)
            {
                AUploadNodeV2 compiled = CompileSingleNode(node, infos, state);
                if (compiled == null)
                {
                    return null;
                }

                steps.Add(compiled);
                node = GetNextNode(node);
            }

            return steps.Count == 1 ? steps[0] : new SequenceNodeV2(steps);
        }

        private static AUploadNodeV2 CompileSingleNode(INode node, GraphLogger infos, CompileState state)
        {
            switch (node)
            {
                case BuildProfileSourceNode sourceNode:
                    return CompileBuildProfileSource(sourceNode, infos, state);

                case LocalPathDestinationNode destinationNode:
                    return CompileLocalPathDestination(destinationNode, infos, state);

                case DebugLogNode debugLogNode:
                    return CompileDebugLog(debugLogNode, infos, state);

                case SetVariableNode setVariableNode:
                    return CompileSetVariable(setVariableNode, infos, state);

                case GetVariableNode:
                    infos?.LogError("A GetVariableNode is a data source, not a step - wire it into another node's input instead of the Execution chain.", node);
                    return null;

                case SequentialGroupNode sequentialGroupNode:
                    return CompileGroup(sequentialGroupNode, isParallel: false, infos, state);

                case ParallelGroupNode parallelGroupNode:
                    return CompileGroup(parallelGroupNode, isParallel: true, infos, state);

                case StartNode:
                    infos?.LogError("A StartNode cannot be wired to anything other than the graph's entry point.", node);
                    return null;

                default:
                    infos?.LogError($"Unsupported node type in graph: {node.GetType().Name}", node);
                    return null;
            }
        }

        private static AUploadNodeV2 CompileGroup(AGroupNode groupNode, bool isParallel, GraphLogger infos, CompileState state)
        {
            int branchCount = groupNode.GetBranchCount();
            List<AUploadNodeV2> branches = new List<AUploadNodeV2>();

            for (int i = 0; i < branchCount; i++)
            {
                IPort branchPort = groupNode.GetOutputPortByName(AGroupNode.BRANCH_PORT_PREFIX + i);
                INode branchStart = branchPort?.firstConnectedPort?.GetNode();
                if (branchStart == null)
                {
                    // An empty branch is fine - it just does nothing.
                    continue;
                }

                AUploadNodeV2 branchTree = CompileChain(branchStart, infos, state);
                if (branchTree == null)
                {
                    return null;
                }

                branches.Add(branchTree);
            }

            if (branches.Count == 0)
            {
                infos?.LogWarning("Group node has no connected branches - it will do nothing.", groupNode);
                return new SequenceNodeV2(new List<AUploadNodeV2>());
            }

            return isParallel
                ? (AUploadNodeV2)new ParallelNodeV2(branches)
                : new SequenceNodeV2(branches);
        }

        private static AUploadNodeV2 CompileBuildProfileSource(BuildProfileSourceNode node, GraphLogger infos, CompileState state)
        {
            BuildProfile profile = GetInputPortValue<BuildProfile>(node.GetInputPortByName(BuildProfileSourceNode.IN_PORT_BUILD_PROFILE));
            bool cleanBuild = GetInputPortValue<bool>(node.GetInputPortByName(BuildProfileSourceNode.IN_PORT_CLEAN_BUILD));
            if (profile == null)
            {
                infos?.LogError("BuildProfileSourceNode has no Build Profile assigned.", node);
                return null;
            }

            BuildProfileSource source = new BuildProfileSource(profile, cleanBuild);
            state.Sources.Add(source);
            state.RegisterOutput(node, BuildProfileSourceNode.OUT_PORT_OUTPUT_PATH, source.SourceFilePath);

            return new SourceNodeV2(source, $"BuildProfileSourceNode ({profile.name})");
        }

        private static AUploadNodeV2 CompileLocalPathDestination(LocalPathDestinationNode node, GraphLogger infos, CompileState state)
        {
            string localPath = GetInputPortValue<string>(node.GetInputPortByName(LocalPathDestinationNode.IN_PORT_LOCAL_PATH));
            if (string.IsNullOrEmpty(localPath))
            {
                infos?.LogError("LocalPathDestinationNode has no Local Path set.", node);
                return null;
            }

            LocalPathDestination destination = new LocalPathDestination(localPath);
            state.Destinations.Add(destination);
            state.RegisterOutput(node, LocalPathDestinationNode.OUT_PORT_OUTPUT_PATH, destination.FullPath);
            state.RegisterOutput(node, LocalPathDestinationNode.OUT_PORT_TOTAL_BYTES,
                () => ComputeTotalBytes(destination.FullPath()).ToString());

            IPort contentPathPort = node.GetInputPortByName(LocalPathDestinationNode.IN_PORT_CONTENT_PATH);
            if (!contentPathPort.isConnected)
            {
                infos?.LogWarning("LocalPathDestinationNode's Content Path isn't wired to anything - it will fail at " +
                                   "runtime with nothing to copy. Wire it to a source's Output Path (or a variable that " +
                                   "resolves to one).", node);
            }

            Func<string> contentPathProvider = ResolveConnectedProvider(contentPathPort, infos, state);

            return new DestinationNodeV2(destination, contentPathProvider, $"LocalPathDestinationNode ({localPath})");
        }

        private static AUploadNodeV2 CompileDebugLog(DebugLogNode node, GraphLogger infos, CompileState state)
        {
            IPort messagePort = node.GetInputPortByName(DebugLogNode.IN_PORT_MESSAGE);
            Func<string> messageProvider = ResolveConnectedProvider(messagePort, infos, state);

            DebugLogAction action;
            if (messageProvider != null)
            {
                action = new DebugLogAction(messageProvider);
            }
            else
            {
                messagePort.TryGetValue(out string literal);
                action = new DebugLogAction(literal ?? string.Empty);
            }

            state.Actions.Add(action);
            return new ActionNodeV2(action, "DebugLogNode");
        }

        /// <summary>
        /// Compiles to a <see cref="SetVariableNodeV2"/> and, exactly like every other "produced now, consumed
        /// later" value in this codebase (see e.g. SlackMessageChannelAction.StringContextModifier.cs), registers
        /// ONE Context command up front whose getter reads the node's live <see cref="SetVariableNodeV2.CurrentValue"/>.
        /// Because it's a real Context command, "{name}" also resolves in any other literal string field that
        /// already runs through Context.FormatString (e.g. a LocalPathDestinationNode's Local Path) - not just
        /// through an explicit GetVariableNode wire.
        /// </summary>
        private static AUploadNodeV2 CompileSetVariable(SetVariableNode node, GraphLogger infos, CompileState state)
        {
            IPort namePort = node.GetInputPortByName(SetVariableNode.IN_PORT_NAME);
            if (!TryGetWiredVariableName(namePort, out string name))
            {
                infos?.LogError("SetVariableNode's Variable must be wired to a Blackboard variable node (drag one from the Blackboard onto the graph and connect it) - Graph Toolkit has no built-in dropdown for this, so a typed name isn't accepted.", node);
                return null;
            }

            IPort valuePort = node.GetInputPortByName(SetVariableNode.IN_PORT_VALUE);
            Func<string> valueProvider = ResolveConnectedProvider(valuePort, infos, state);
            if (valueProvider == null)
            {
                valuePort.TryGetValue(out string literal);
                string capturedLiteral = literal ?? string.Empty;
                valueProvider = () => capturedLiteral;
            }

            SetVariableNodeV2 setVariableNode = new SetVariableNodeV2(name, valueProvider);
            state.Task.Context.AddCommand("{" + name + "}", () => setVariableNode.CurrentValue);

            return setVariableNode;
        }

        /// <summary>
        /// If <paramref name="inputPort"/> is connected, returns a lazily-evaluated provider for its value.
        /// Blackboard variables and constant nodes resolve immediately (their value is known at compile time);
        /// a <see cref="GetVariableNode"/> resolves through the task's <see cref="Context"/> (see
        /// <see cref="CompileSetVariable"/>); anything else is assumed to be one of our own compiled nodes and is
        /// looked up via <see cref="CompileState.RegisterOutput"/> at call time - i.e. only once that node has
        /// actually run. Returns null if the port isn't connected (caller should fall back to its literal value).
        /// </summary>
        private static Func<string> ResolveConnectedProvider(IPort inputPort, GraphLogger infos, CompileState state)
        {
            if (!inputPort.isConnected)
            {
                return null;
            }

            if (TryGetConnectedConstantValue(inputPort, out string constantValue))
            {
                return () => constantValue;
            }

            IPort upstreamPort = inputPort.firstConnectedPort;
            INode upstreamNode = upstreamPort.GetNode();

            if (upstreamNode is GetVariableNode getVariableNode)
            {
                IPort namePort = getVariableNode.GetInputPortByName(GetVariableNode.IN_PORT_NAME);
                if (!TryGetWiredVariableName(namePort, out string variableName))
                {
                    infos?.LogError("GetVariableNode's Variable must be wired to a Blackboard variable node (drag one from the Blackboard onto the graph and connect it) - Graph Toolkit has no built-in dropdown for this, so a typed name isn't accepted.", getVariableNode);
                    return () => string.Empty;
                }

                string key = "{" + variableName + "}";
                return () => state.Task.Context.FormatString(key);
            }

            string upstreamPortName = upstreamPort.name;

            return () =>
            {
                Func<string> provider = state.GetOutputProvider(upstreamNode, upstreamPortName);
                return provider != null ? provider() : "";
            };
        }

        /// <summary>
        /// Reads the name of the Blackboard variable node wired into <paramref name="namePort"/>. This is how
        /// <see cref="SetVariableNode"/>/<see cref="GetVariableNode"/> pick which variable "slot" to use - Graph
        /// Toolkit has no dropdown for a dynamic list of names, so dragging a variable from the Blackboard and
        /// wiring it in is the supported stand-in. Only the variable's name is used; its declared value/type isn't.
        /// </summary>
        private static bool TryGetWiredVariableName(IPort namePort, out string variableName)
        {
            if (namePort.isConnected && namePort.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableName = variableNode.variable.name;
                return !string.IsNullOrEmpty(variableName);
            }

            variableName = null;
            return false;
        }

        private static INode GetNextNode(INode currentNode)
        {
            IPort outputPort = currentNode.GetOutputPortByName(ABuildUploaderNode.EXECUTION_PORT_NAME);
            IPort nextPort = outputPort?.firstConnectedPort;
            return nextPort?.GetNode();
        }

        /// <summary>
        /// Reads a port's value: a Blackboard variable or constant node if it's wired to one, otherwise the
        /// port's own embedded/literal value (the normal case for an unconnected input port).
        /// </summary>
        private static T GetInputPortValue<T>(IPort port)
        {
            if (TryGetConnectedConstantValue(port, out T connectedValue))
            {
                return connectedValue;
            }

            port.TryGetValue(out T value);
            return value;
        }

        /// <summary>
        /// If <paramref name="port"/> is connected to a Blackboard variable node or a constant node, retrieves
        /// that value directly. Unlike our own nodes' outputs (only known once they've run), a variable's default
        /// value and a constant's value both exist immediately, before anything in the graph has executed.
        /// </summary>
        private static bool TryGetConnectedConstantValue<T>(IPort port, out T value)
        {
            if (port.isConnected)
            {
                switch (port.firstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        return variableNode.variable.TryGetDefaultValue(out value);
                    case IConstantNode constantNode:
                        return constantNode.TryGetValue(out value);
                }
            }

            value = default;
            return false;
        }

        private static long ComputeTotalBytes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }

            if (Directory.Exists(path))
            {
                return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            }

            return 0;
        }
    }
}
