using System.Collections.Generic;
using System.IO;
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
    [Wiki(nameof(LastBuildSource), "sources", "Chooses the directory of the last build made using the Build Uploader")]
    [BuildSource("LastBuildSource", "Last Build Directory")]
    public class LastBuildSource : ABuildSource
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

        public override async Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult,
            StringFormatter.Context ctx)
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

        public override float DownloadProgress()
        {
            return 0.0f; // No download progress for last build source
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