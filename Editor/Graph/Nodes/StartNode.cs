using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// The single entry point of a <see cref="BuildUploaderGraph"/>.
    /// A graph must contain exactly one of these - see <see cref="BuildUploaderGraph.OnGraphChanged"/>.
    /// </summary>
    [Serializable]
    public class StartNode : ABuildUploaderNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            // Start is a special node that has no input - it is where execution begins.
            AddExecutionPorts(context, hasInput: false, hasOutput: true);
        }
    }
}
