#if BUILD_UPLOADER_GRAPHTOOLKIT
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Executes a compiled graph. Groups run in flow order (each awaited before the next, so the chain is sequential).
    /// Within a group:
    ///   Parallel  → all configs run together in one <see cref="UploadTask"/>.
    ///   Sequential → one <see cref="UploadTask"/> per config, awaited in block order.
    /// This delivers real parallel/sequential behaviour through the unchanged execution engine.
    /// </summary>
    public static class BuildUploaderGraphRunner
    {
        public static async Task RunAsync(BuildUploaderGraph graph)
        {
            GraphCompileLog log = new GraphCompileLog();
            GraphPlan plan = BuildUploaderGraphCompiler.Compile(graph, log);

            foreach (string warning in log.Warnings)
            {
                Debug.LogWarning($"[Build Uploader Graph] {warning}");
            }

            if (log.HasErrors || plan == null)
            {
                foreach (string error in log.Errors)
                {
                    Debug.LogError($"[Build Uploader Graph] {error}");
                }
                return;
            }

            foreach (GroupPlan group in plan.Groups)
            {
                if (group.Mode == GroupMode.Parallel)
                {
                    UploadTask task = new UploadTask(group.Name, group.Configs);
                    await task.StartAsync();
                }
                else
                {
                    foreach (UploadConfig config in group.Configs)
                    {
                        UploadTask task = new UploadTask(group.Name, new List<UploadConfig> { config });
                        await task.StartAsync();
                    }
                }
            }
        }

        [MenuItem("Assets/Build Uploader/Run Upload Graph", true)]
        private static bool ValidateRunSelectedGraph()
        {
            return Selection.activeObject != null &&
                   AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith("." + BuildUploaderGraph.AssetExtension);
        }

        [MenuItem("Assets/Build Uploader/Run Upload Graph", false)]
        private static void RunSelectedGraph()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            BuildUploaderGraph graph = GraphDatabase.LoadGraph<BuildUploaderGraph>(path);
            _ = RunAsync(graph);
        }
    }
}
#endif
