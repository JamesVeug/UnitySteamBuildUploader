using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class NintendoUploadDestination
    {
        private bool m_showFormattedDescription = Preferences.DefaultShowFormattedTextToggle;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            float segmentLength = maxWidth / 2f;

            isDirty |= NintendoUIUtils.ConfigPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(segmentLength));
            isDirty |= NintendoUIUtils.BranchPopup.DrawPopup(m_app, ref m_destinationBranch, m_context, GUILayout.Width(segmentLength));
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Title:", "The Nintendo Title (game) to upload to. This is the Title configured in the Nintendo Developer Center.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= NintendoUIUtils.ConfigPopup.DrawPopup(ref m_app, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Branch:", "The release Branch / ring to upload to (for example master, public-beta, internal-test).");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= NintendoUIUtils.BranchPopup.DrawPopup(m_app, ref m_destinationBranch, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Description Format:", "Description for developers that appears in the Nintendo Developer Center.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextArea(ref m_descriptionFormat, ref m_showFormattedDescription, m_context);
            }
        }

        public override string Summary()
        {
            string title = m_uploadApp?.DisplayName ?? m_app?.DisplayName ?? "Unspecified Title";
            string branch = m_uploadBranch?.DisplayName ?? m_destinationBranch?.DisplayName ?? "Unspecified Branch";
            return $"Title: {title} Branch: {branch}";
        }
    }
}
