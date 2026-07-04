using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Runs a <see cref="BuildUploaderGraph"/> asset through <see cref="UploadTaskV2"/>.
    /// </summary>
    public static class BuildUploaderGraphRunner
    {
        [MenuItem("Assets/Build Uploader/Run Graph", true)]
        static bool ValidateRun()
        {
            return TryGetSelectedGraphPath(out _);
        }

        [MenuItem("Assets/Build Uploader/Run Graph")]
        static void Run()
        {
            if (!TryGetSelectedGraphPath(out string assetPath))
            {
                return;
            }

            BuildUploaderGraph graph = GraphDatabase.LoadGraph<BuildUploaderGraph>(assetPath);
            if (graph == null)
            {
                Debug.LogError($"Failed to load Build Uploader Graph asset: {assetPath}");
                return;
            }

            Run(graph);
        }

        public static void Run(BuildUploaderGraph graph)
        {
            if (!BuildUploaderGraphCompiler.TryCompile(graph, out UploadTaskV2 task))
            {
                Debug.LogError("Build Uploader Graph failed to compile. Fix the errors shown in the graph editor and try again.");
                return;
            }

            task.OnComplete += report => Debug.Log(report.GetReport());
            task.Start();
        }

        static bool TryGetSelectedGraphPath(out string assetPath)
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
            {
                assetPath = null;
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(path) || !path.EndsWith("." + BuildUploaderGraph.AssetExtension, StringComparison.OrdinalIgnoreCase))
            {
                assetPath = null;
                return false;
            }

            assetPath = path;
            return true;
        }
    }
}
