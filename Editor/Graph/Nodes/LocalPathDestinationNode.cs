using System;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Wraps <see cref="LocalPathDestination"/> so it can be used as a node in a <see cref="BuildUploaderGraph"/>.
    /// Does not modify or subclass <see cref="LocalPathDestination"/> - <see cref="BuildUploaderGraphCompiler"/>
    /// reads this node's ports and constructs a real <see cref="LocalPathDestination"/> instance to run through
    /// the existing <see cref="UploadTask"/> pipeline.
    /// </summary>
    [Serializable]
    public class LocalPathDestinationNode : ABuildUploaderNode
    {
        public const string IN_PORT_CONTENT_PATH = "ContentPath";
        public const string IN_PORT_LOCAL_PATH = "LocalPath";
        public const string OUT_PORT_OUTPUT_PATH = "OutputPath";
        public const string OUT_PORT_TOTAL_BYTES = "TotalBytes";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);

            // What gets copied - normally wired from an upstream source node's Output Path
            context.AddInputPort<string>(IN_PORT_CONTENT_PATH)
                .WithDisplayName("Content Path")
                .Build();

            // Where LocalPathDestination will copy the content to
            context.AddInputPort<string>(IN_PORT_LOCAL_PATH)
                .WithDisplayName("Local Path")
                .Build();

            // Where the content ended up - equivalent to LocalPathDestination.FullPath()
            context.AddOutputPort<string>(OUT_PORT_OUTPUT_PATH)
                .WithDisplayName("Output Path")
                .Build();

            // Total size on disk of what was copied.
            // NOTE: typed as string (not long) so it can actually be wired into a DebugLogNode's Message -
            // Graph Toolkit only allows connecting ports of the same (or a derived) data type, and every other
            // dynamic value in this graph (paths, messages) is a string, so this stays consistent with those.
            context.AddOutputPort<string>(OUT_PORT_TOTAL_BYTES)
                .WithDisplayName("Total Bytes")
                .Build();
        }
    }
}
