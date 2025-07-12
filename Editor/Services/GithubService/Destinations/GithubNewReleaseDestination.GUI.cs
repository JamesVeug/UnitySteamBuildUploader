using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GithubNewReleaseDestination
    {
        private bool m_showFormattedOwner;
        private bool m_showFormattedRepo;
        private bool m_showFormattedReleaseName;
        private bool m_showFormattedTagName;
        private bool m_showFormattedTarget;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            string owner = StringFormatter.FormatString(m_owner);
            string repo = StringFormatter.FormatString(m_repo);
            string tagName = StringFormatter.FormatString(m_tagName);
            string target = StringFormatter.FormatString(m_target);
            
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
                isDirty |= EditorUtils.FormatStringTextField(ref m_owner, ref m_showFormattedOwner);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Repo:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_repo, ref m_showFormattedRepo);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Release Name:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_releaseName, ref m_showFormattedReleaseName);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Tag:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_tagName, ref m_showFormattedTagName);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch or Commit:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_target, ref m_showFormattedTarget);
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
        }
    }
}