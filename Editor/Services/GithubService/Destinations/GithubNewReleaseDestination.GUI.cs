using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GithubNewReleaseDestination
    {
        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            string text = $"{m_owner}/{m_repo}/releases/tag/{m_tagName} ({m_target})";
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
                isDirty |= CustomTextField.Draw(ref m_owner);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Repo:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_repo);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Release Name:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_releaseName);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Tag:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_tagName);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch or Commit:", GUILayout.Width(120));
                isDirty |= CustomTextField.Draw(ref m_target);
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