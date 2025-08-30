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
    [Wiki(nameof(LastBuildSource), "sources", "Chooses the directory of the last build made using the Build Uploader")]
    [UploadSource("LastBuildSource", "Last Build Directory")]
    public class LastBuildSource : AUploadSource
    {
        public override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            EditorGUILayout.LabelField("Build Name:", LastBuildUtil.LastBuildName);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Directory:", LastBuildUtil.LastBuildDirectory);
                if (GUILayout.Button("Show", GUILayout.Width(100)))
                {
                    if (!string.IsNullOrEmpty(LastBuildUtil.LastBuildDirectory))
                    {
                        EditorUtility.RevealInFinder(LastBuildUtil.LastBuildDirectory);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error",
                            "No last build directory found. Please build your project first.", "OK");
                    }
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            EditorGUILayout.LabelField(LastBuildUtil.LastBuildName, GUILayout.Width(100));
            EditorGUILayout.LabelField(LastBuildUtil.LastBuildDirectory, GUILayout.MaxWidth(maxWidth - 100));
        }

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Wait for our turn if we need to
            await BuildConfigSource.m_lock.WaitAsync();

            try
            {
                if (string.IsNullOrEmpty(LastBuildUtil.LastBuildDirectory))
                {
                    stepResult.AddError("No last build directory found. Please build your project first.");
                    return false;
                }

                if (!Directory.Exists(LastBuildUtil.LastBuildDirectory))
                {
                    stepResult.AddError($"Last build directory does not exist: {LastBuildUtil.LastBuildDirectory}");
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
            return LastBuildUtil.LastBuildDirectory;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (string.IsNullOrEmpty(LastBuildUtil.LastBuildDirectory))
            {
                errors.Add("No last build directory found. Please build your project first.");
            }
            else if (!Directory.Exists(LastBuildUtil.LastBuildDirectory))
            {
                errors.Add($"Last build directory does not exist: {LastBuildUtil.LastBuildDirectory}");
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