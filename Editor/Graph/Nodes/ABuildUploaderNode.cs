using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Base node for every node in a <see cref="BuildUploaderGraph"/>.
    /// Provides the shared "Execution" control-flow ports so <see cref="BuildUploaderGraphCompiler"/>
    /// can walk the graph the same way regardless of node type.
    /// </summary>
    [Serializable]
    public abstract class ABuildUploaderNode : Node
    {
        public const string EXECUTION_PORT_NAME = "Execution";

        protected void AddExecutionPorts(IPortDefinitionContext context, bool hasInput = true, bool hasOutput = true)
        {
            if (hasInput)
            {
                context.AddInputPort(EXECUTION_PORT_NAME)
                    .WithDisplayName(string.Empty)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }

            if (hasOutput)
            {
                context.AddOutputPort(EXECUTION_PORT_NAME)
                    .WithDisplayName(string.Empty)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }
    }
}
