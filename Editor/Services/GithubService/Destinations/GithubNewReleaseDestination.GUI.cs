using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GithubNewReleaseDestination
    {
        private bool m_showFormattedOwner = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedRepo = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedReleaseName = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedTagName = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedTarget = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedDescription = Preferences.DefaultShowFormattedTextToggle;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            string owner = m_context.FormatString(m_owner);
            string repo = m_context.FormatString(m_repo);
            string tagName = m_context.FormatString(m_tagName);
            string target = m_context.FormatString(m_target);
            
            string text = $"{owner}/{repo}/releases/tag/{tagName} ({target})";
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                Application.OpenURL(
                    "https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#create-a-release");
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Owner:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_owner, ref m_showFormattedOwner, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Repo:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_repo, ref m_showFormattedRepo, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Release Name:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_releaseName, ref m_showFormattedReleaseName, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Tag:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_tagName, ref m_showFormattedTagName, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Target Commitish:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_target, ref m_showFormattedTarget, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Draft:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_draft);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Prerelease:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_prerelease);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_zipContents);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Description Format:", "Description to appears on the package.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextArea(ref m_descriptionFormat, ref m_showFormattedDescription, m_context);
            }
        }
    }
}