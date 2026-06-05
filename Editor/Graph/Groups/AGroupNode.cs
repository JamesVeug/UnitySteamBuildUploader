#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Base for the group context nodes. A group is a Graph Toolkit <see cref="ContextNode"/> that contains operation
    /// <see cref="BlockNode"/>s and runs them according to <see cref="Mode"/>. It exposes an exec "In" and "Then" so
    /// groups can be chained after the Start node and after each other.
    /// </summary>
    [Serializable]
    public abstract class AGroupNode : ContextNode
    {
        public const string PortIn = "In";
        public const string PortThen = "Then";

        public abstract GroupMode Mode { get; }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<ExecFlow>(PortIn).WithDisplayName("In").Build();
            context.AddOutputPort<ExecFlow>(PortThen).WithDisplayName("Then").Build();
        }
    }
}
#endif
