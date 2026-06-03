using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class NintendoNotifyAction
    {
        private bool m_showFormattedText = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedDescription = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedIdFormat = Preferences.DefaultShowFormattedTextToggle;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= NintendoUIUtils.ConfigPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(120));
            isDirty |= NintendoUIUtils.BranchPopup.DrawPopup(m_app, ref m_branch, m_context, GUILayout.Width(120));

            float width = maxWidth - (120 * 2);
            using (new EditorGUI.DisabledScope(true))
            {
                bool alwaysFormatted = true;
                EditorUtils.FormatStringTextArea(ref m_text, ref alwaysFormatted, m_context, null, GUILayout.Width(width));
            }
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Title:", GUILayout.Width(60));
                isDirty |= NintendoUIUtils.ConfigPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(120));

                GUILayout.Label("Branch:", GUILayout.Width(60));
                if (m_app == null)
                {
                    GUILayout.Label("No Title selected", GUILayout.Width(120));
                }
                else
                {
                    isDirty |= NintendoUIUtils.BranchPopup.DrawPopup(m_app, ref m_branch, m_context, GUILayout.Width(120));
                }
            }

            var idFormatLabel = new GUIContent("Message ID Format", idFormatTooltip);
            GUILayout.Label(idFormatLabel, GUILayout.Width(200));
            isDirty |= ContextGUI.DrawKey(m_responseIdFormat, ref m_showFormattedIdFormat, m_context);

            GUILayout.Label("Text:", GUILayout.Width(50));
            if (EditorUtils.FormatStringTextArea(ref m_text, ref m_showFormattedText, m_context))
            {
                isDirty = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Description Format:", "Description sent alongside the notification payload."), GUILayout.Width(150));
                isDirty |= EditorUtils.FormatStringTextArea(ref m_descriptionFormat, ref m_showFormattedDescription, m_context);
            }
        }
    }
}
