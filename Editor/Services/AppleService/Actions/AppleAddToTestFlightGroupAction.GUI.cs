using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class AppleAddToTestFlightGroupAction
    {
        private bool m_showFormattedBuildId = Preferences.DefaultShowFormattedTextToggle;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= AppleUIUtils.ApiKeyPopup.DrawPopup(ref m_apiKey, m_context, GUILayout.Width(120));
            isDirty |= AppleUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(120));

            int groupCount = m_betaGroups != null ? m_betaGroups.Count : 0;
            GUILayout.Label($"{groupCount} group(s)", GUILayout.Width(80));
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("API Key:", GUILayout.Width(120));
                isDirty |= AppleUIUtils.ApiKeyPopup.DrawPopup(ref m_apiKey, m_context, GUILayout.Width(200));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= AppleUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(200));
            }

            GUILayout.Label("Beta Groups:", GUILayout.Width(120));
            DrawBetaGroupsToggleList(ref isDirty);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Build ID Format:",
                    "Resolves to the App Store Connect Build resource ID. Default {appleBuildId} is populated when uploading a build to TestFlight.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextArea(ref m_buildIdFormat, ref m_showFormattedBuildId, m_context);
            }
        }

        private void DrawBetaGroupsToggleList(ref bool isDirty)
        {
            if (m_app == null)
            {
                EditorGUILayout.HelpBox("Select an App to choose its beta groups.", MessageType.Info);
                return;
            }

            if (m_betaGroups == null)
            {
                m_betaGroups = new System.Collections.Generic.List<AppleConfig.AppleBetaGroup>();
            }

            // Drop any stale references that no longer exist on the app.
            int removed = m_betaGroups.RemoveAll(g => !m_app.betaGroups.Contains(g));
            if (removed > 0) isDirty = true;

            if (m_app.betaGroups.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "This app has no beta groups configured. Add them in Project Settings -> Build Uploader -> Services -> Apple.",
                    MessageType.Info);
                return;
            }

            foreach (AppleConfig.AppleBetaGroup group in m_app.betaGroups)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool active = m_betaGroups.Contains(group);
                    bool newActive = GUILayout.Toggle(active, group.DisplayName, GUILayout.Width(200));
                    if (newActive != active)
                    {
                        isDirty = true;
                        if (newActive)
                        {
                            if (!m_betaGroups.Contains(group))
                            {
                                m_betaGroups.Add(group);
                            }
                        }
                        else
                        {
                            m_betaGroups.Remove(group);
                        }
                    }
                }
            }
        }
    }
}
