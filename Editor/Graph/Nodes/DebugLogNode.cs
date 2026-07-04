using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Logs a message to the console when reached during execution.
    /// The Message port can be a literal string or wired to an upstream node's output
    /// (e.g. a <see cref="BuildProfileSourceNode"/>'s Output Path or a
    /// <see cref="LocalPathDestinationNode"/>'s Output Path / Total Bytes).
    /// Any number of these can appear in a graph.
    /// Compiles to a <see cref="DebugLogAction"/> run through the normal UploadTask PostActions step.
    /// </summary>
    [Serializable]
    public class DebugLogNode : ABuildUploaderNode
    {
        public const string IN_PORT_MESSAGE = "Message";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);

            context.AddInputPort<string>(IN_PORT_MESSAGE)
                .WithDisplayName("Message")
                .Build();
        }
    }
}
