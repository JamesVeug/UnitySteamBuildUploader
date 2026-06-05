#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// Node-based authoring surface for the Build Uploader pipeline.
    ///
    /// A graph is a visual editor for one or more <see cref="UploadConfig"/>s: Source nodes feed Destination
    /// nodes, which fan out to Action nodes. <see cref="BuildUploaderGraphCompiler"/> bakes the authored graph
    /// down to the existing <see cref="UploadProfile"/> / <see cref="UploadConfig"/> model so it runs through the
    /// unchanged <see cref="UploadTask"/> orchestration (and therefore through BatchModeUtil / CLI too).
    ///
    /// The graph asset is the authoring source of truth only. Nothing here re-implements pipeline execution.
    ///
    /// NOTE: Graph Toolkit (com.unity.graphtoolkit) is experimental. This whole assembly compiles out unless the
    /// package is installed (BUILD_UPLOADER_GRAPHTOOLKIT is auto-defined by the asmdef versionDefine).
    /// </summary>
    [Graph(AssetExtension)]
    [Serializable]
    public class BuildUploaderGraph : Graph
    {
        public const string AssetExtension = "bugraph";

        [MenuItem("Assets/Create/Build Uploader/Upload Graph", false)]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BuildUploaderGraph>();
        }

        /// <summary>
        /// Surfaces compile errors/warnings as graph validation in the editor so the user sees configuration
        /// problems up front, mirroring the pipeline's own Validation-step contract.
        /// </summary>
        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            GraphCompileLog log = new GraphCompileLog();
            BuildUploaderGraphCompiler.Compile(this, log);

            foreach (string error in log.Errors)
            {
                graphLogger.LogError(error);
            }

            foreach (string warning in log.Warnings)
            {
                graphLogger.LogWarning(warning);
            }
        }
    }
}
#endif
