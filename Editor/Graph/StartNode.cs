#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Entry point of an upload graph. Wire its "Start" output into the first group's "In" port; the compiler walks
    /// the exec chain from here to determine group order.
    /// </summary>
    [Serializable]
    public class StartNode : Node
    {
        public const string PortStart = "Start";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<ExecFlow>(PortStart).WithDisplayName("Start").Build();
        }
    }
}
#endif
