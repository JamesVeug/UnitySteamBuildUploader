using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class LastBuildSource
    {
        public override void OnGUIExpanded(ref bool isDirty, UploadConfig.SourceData data)
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

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            EditorGUILayout.LabelField(LastBuildUtil.LastBuildName, GUILayout.Width(100));
            EditorGUILayout.LabelField(LastBuildUtil.LastBuildDirectory, GUILayout.MaxWidth(maxWidth - 100));
        }

        public override string Summary()
        {
            return LastBuildUtil.LastBuildDirectory;
        }
    }
}