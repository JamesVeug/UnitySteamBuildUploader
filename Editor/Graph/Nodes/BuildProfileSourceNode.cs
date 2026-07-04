using System;
using Unity.GraphToolkit.Editor;
using UnityEditor.Build.Profile;

namespace Wireframe
{
    /// <summary>
    /// Wraps <see cref="BuildProfileSource"/> so it can be used as a node in a <see cref="BuildUploaderGraph"/>.
    /// Does not modify or subclass <see cref="BuildProfileSource"/> - <see cref="BuildUploaderGraphCompiler"/>
    /// reads this node's ports and constructs a real <see cref="BuildProfileSource"/> instance to run through
    /// the existing <see cref="UploadTask"/> pipeline.
    /// </summary>
    [Serializable]
    public class BuildProfileSourceNode : ABuildUploaderNode
    {
        public const string IN_PORT_BUILD_PROFILE = "BuildProfile";
        public const string IN_PORT_CLEAN_BUILD = "CleanBuild";
        public const string OUT_PORT_OUTPUT_PATH = "OutputPath";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);

            // Required fields of BuildProfileSource
            context.AddInputPort<BuildProfile>(IN_PORT_BUILD_PROFILE)
                .WithDisplayName("Build Profile")
                .Build();
            context.AddInputPort<bool>(IN_PORT_CLEAN_BUILD)
                .WithDisplayName("Clean Build")
                .WithDefaultValue(false)
                .Build();

            // The path that the build was saved to - equivalent to AUploadSource.SourceFilePath()
            context.AddOutputPort<string>(OUT_PORT_OUTPUT_PATH)
                .WithDisplayName("Output Path")
                .Build();
        }
    }
}
