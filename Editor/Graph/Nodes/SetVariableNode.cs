using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Stores a value, keyed by a Blackboard variable's name, in the running <see cref="UploadTaskV2"/> when
    /// reached during execution, so a <see cref="GetVariableNode"/> anywhere later in the same run can read it back.
    ///
    /// The value itself is real runtime state (fresh per run) - deliberately separate from Graph Toolkit's
    /// Blackboard variables, which only carry an edit-time default (there's no supported way to write to one while
    /// a graph is running; see <see cref="IVariable.TryGetDefaultValue{T}"/>, which has no matching setter).
    ///
    /// The Blackboard is only used here to pick *which* variable slot to write to: Graph Toolkit has no built-in
    /// dropdown for a dynamic list of names, so instead of typing a name (and risking typos), wire an actual
    /// variable node dragged from the Blackboard into the "Variable" port - see
    /// <see cref="BuildUploaderGraphCompiler.TryGetWiredVariableName"/>. Only the variable's name is used; its own
    /// declared value/type is irrelevant and unused.
    /// </summary>
    [Serializable]
    public class SetVariableNode : ABuildUploaderNode
    {
        public const string IN_PORT_NAME = "Name";
        public const string IN_PORT_VALUE = "Value";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);

            context.AddInputPort<string>(IN_PORT_NAME)
                .WithDisplayName("Variable")
                .Build();

            context.AddInputPort<string>(IN_PORT_VALUE)
                .WithDisplayName("Value")
                .Build();
        }
    }
}
