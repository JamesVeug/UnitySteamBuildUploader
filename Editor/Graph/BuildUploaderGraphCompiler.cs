#if BUILD_UPLOADER_GRAPHTOOLKIT
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Compiles a <see cref="BuildUploaderGraph"/> into an ordered <see cref="GraphPlan"/>.
    ///
    /// Walks the exec chain from the Start node (Start → group → group …). Each group is a context node whose
    /// operation blocks compile to <see cref="UploadConfig"/>s (copy blocks) or post-actions (action blocks, attached
    /// to the nearest preceding copy config). The plan keeps each group's <see cref="GroupMode"/> so the runner can
    /// honour Sequential vs Parallel without changing the execution engine.
    /// </summary>
    public static class BuildUploaderGraphCompiler
    {
        public static GraphPlan Compile(BuildUploaderGraph graph, GraphCompileLog log)
        {
            if (graph == null)
            {
                log.Error("No graph supplied.");
                return null;
            }

            StartNode start = null;
            foreach (INode node in graph.GetNodes())
            {
                if (node is StartNode s)
                {
                    if (start != null)
                    {
                        log.Warning("Multiple Start nodes found; using the first one.");
                        break;
                    }
                    start = s;
                }
            }

            if (start == null)
            {
                log.Error("Graph has no Start node.");
                return null;
            }

            GraphPlan plan = new GraphPlan();

            // Follow the exec chain: Start.Start -> Group.In, then Group.Then -> next Group.In.
            HashSet<AGroupNode> visited = new HashSet<AGroupNode>();
            IPort exec = start.GetOutputPortByName(StartNode.PortStart);
            while (exec != null)
            {
                AGroupNode group = FindConnectedGroup(exec);
                if (group == null || !visited.Add(group))
                {
                    break;
                }

                plan.Groups.Add(CompileGroup(group, log));
                exec = group.GetOutputPortByName(AGroupNode.PortThen);
            }

            if (plan.Groups.Count == 0)
            {
                log.Error("Start node is not connected to any group, so nothing will run.");
                return null;
            }

            return plan;
        }

        private static AGroupNode FindConnectedGroup(IPort execOutput)
        {
            List<IPort> connected = new List<IPort>();
            execOutput.GetConnectedPorts(connected);
            foreach (IPort port in connected)
            {
                if (port.GetNode() is AGroupNode group)
                {
                    return group;
                }
            }

            return null;
        }

        private static GroupPlan CompileGroup(AGroupNode group, GraphCompileLog log)
        {
            GroupPlan groupPlan = new GroupPlan
            {
                Name = group.GetType().Name,
                Mode = group.Mode,
            };

            UploadConfig lastConfig = null;
            foreach (BlockNode block in group.blockNodes)
            {
                switch (block)
                {
                    case ICopyBlock copyBlock:
                        lastConfig = copyBlock.CompileConfig(log);
                        groupPlan.Configs.Add(lastConfig);
                        break;

                    case IActionBlock actionBlock:
                        if (lastConfig == null)
                        {
                            log.Error($"{groupPlan.Name}: '{block.GetType().Name}' has no preceding copy operation to attach to.");
                        }
                        else
                        {
                            lastConfig.AddPostAction(actionBlock.CompileAction(log));
                        }
                        break;
                }
            }

            if (groupPlan.Configs.Count == 0)
            {
                log.Error($"{groupPlan.Name}: group contains no copy operations.");
            }

            return groupPlan;
        }
    }
}
#endif
