using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class UnityCloudSource
    {
        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Target:", GUILayout.Width(120));
                isDirty |= UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget, m_context);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Build:", GUILayout.Width(120));
                using (new EditorGUI.DisabledScope(UnityCloudAPI.IsSyncing))
                {
                    if (GUILayout.Button("Refresh", GUILayout.Width(75)))
                    {
                        UnityCloudAPI.SyncBuilds();
                        //UnityCloudAPIEditorUtil.TargetPopup.Refresh();
                        isDirty = true;;
                    }
                }

                buildScrollPosition = EditorGUILayout.BeginScrollView(buildScrollPosition, GUILayout.MaxHeight(100));
                using (new EditorGUILayout.VerticalScope())
                {
                    List<UnityCloudBuild> builds = UnityCloudAPI.GetBuildsForTarget(sourceTarget);
                    if (builds != null)
                    {
                        for (int i = 0; i < builds.Count; i++)
                        {
                            UnityCloudBuild build = builds[i];
                            bool isSelected = sourceBuild != null &&
                                              sourceBuild.CreateBuildName() == build.CreateBuildName();
                            using (new EditorGUI.DisabledScope(isSelected || UnityCloudAPI.IsSyncing))
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(build.CreateBuildName()))
                                    {
                                        sourceBuild = build;
                                        isDirty = true;
                                    }
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            if (UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget, m_context))
            {
                isDirty = true;
            }

            if (UnityCloudAPIEditorUtil.BuildPopup.DrawPopup(sourceTarget, ref sourceBuild, m_context))
            {
                isDirty = true;
            }
        }

        public override string Summary()
        {
            return sourceTarget?.DisplayName;
        }
    }
}