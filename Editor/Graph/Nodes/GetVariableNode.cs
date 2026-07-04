using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Reads a value previously stored by a <see cref="SetVariableNode"/> earlier in the same
    /// <see cref="UploadTaskV2"/> run. Purely a data source - no execution ports - so it can be wired into any
    /// node's string input the same way a Blackboard variable or constant node can (see
    /// <see cref="BuildUploaderGraphCompiler.ResolveConnectedProvider"/>). Resolves to an empty string if nothing
    /// has set that name (yet, or ever) by the time it's read.
    ///
    /// Like <see cref="SetVariableNode"/>, "Variable" must be wired to an actual Blackboard variable node (dragged
    /// from the Blackboard) rather than typed as text - that's the only variable-name picker Graph Toolkit
    /// supports natively. Only the variable's name is used; its own declared value/type is irrelevant and unused.
    /// </summary>
    [Serializable]
    public class GetVariableNode : ABuildUploaderNode
    {
        public const string IN_PORT_NAME = "Name";
        public const string OUT_PORT_VALUE = "Value";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(IN_PORT_NAME)
                .WithDisplayName("Variable")
                .Build();

            context.AddOutputPort<string>(OUT_PORT_VALUE)
                .WithDisplayName("Value")
                .Build();
        }
    }
}
