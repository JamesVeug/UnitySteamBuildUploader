using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// A visual, node-based way to author an upload pipeline using the Graph Toolkit.
    /// This graph is compiled and run through <see cref="UploadTaskV2"/> - see
    /// <see cref="BuildUploaderGraphCompiler"/> for how a graph is turned into a runnable tree of nodes,
    /// and <see cref="BuildUploaderGraphRunner"/> for running one.
    /// </summary>
    [Serializable]
    [Graph(AssetExtension)]
    public class BuildUploaderGraph : Graph
    {
        internal const string AssetExtension = "builduploadergraph";

        [MenuItem("Assets/Create/Build Uploader/Build Uploader Graph")]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BuildUploaderGraph>("Build Uploader Graph");
        }

        public override void OnGraphChanged(GraphLogger infos)
        {
            base.OnGraphChanged(infos);

            BuildUploaderGraphCompiler.Validate(this, infos);
        }
    }
}
