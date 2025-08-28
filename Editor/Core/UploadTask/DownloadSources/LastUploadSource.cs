using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Auto picks the location of the last build made from Unity.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(LastUploadSource), "sources", "Chooses the directory of the last build made using the Build Uploader")]
    [UploadSource("LastBuildSource", "Last Build Directory")]
    public class LastUploadSource : AUploadSource
    {
        public override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            EditorGUILayout.LabelField("Last Build Directory", LastBuildDirectoryUtil.LastBuildDirectory);
            if (GUILayout.Button("Open Last Build Directory"))
            {
                if (!string.IsNullOrEmpty(LastBuildDirectoryUtil.LastBuildDirectory))
                {
                    EditorUtility.RevealInFinder(LastBuildDirectoryUtil.LastBuildDirectory);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No last build directory found. Please build your project first.", "OK");
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            EditorGUILayout.LabelField(LastBuildDirectoryUtil.LastBuildDirectory, GUILayout.MaxWidth(maxWidth));
        }

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Wait for our turn if we need to
            await BuildConfigSource.m_lock.WaitAsync();

            try
            {
                if (string.IsNullOrEmpty(LastBuildDirectoryUtil.LastBuildDirectory))
                {
                    stepResult.AddError("No last build directory found. Please build your project first.");
                    return false;
                }

                if (!Directory.Exists(LastBuildDirectoryUtil.LastBuildDirectory))
                {
                    stepResult.AddError($"Last build directory does not exist: {LastBuildDirectoryUtil.LastBuildDirectory}");
                    return false;
                }
            }
            finally
            {
                BuildConfigSource.m_lock.Release();
            }

            return true;
        }

        public override string SourceFilePath()
        {
            return LastBuildDirectoryUtil.LastBuildDirectory;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (string.IsNullOrEmpty(LastBuildDirectoryUtil.LastBuildDirectory))
            {
                errors.Add("No last build directory found. Please build your project first.");
            }
            else if (!Directory.Exists(LastBuildDirectoryUtil.LastBuildDirectory))
            {
                errors.Add($"Last build directory does not exist: {LastBuildDirectoryUtil.LastBuildDirectory}");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object> { };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            
        }
    }
}