using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GoogleChatSendMessageChannelAction
    {
        private bool m_showFormattedText = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedMessageNameFormat = Preferences.DefaultShowFormattedTextToggle;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= GoogleUIUtils.ChatSpacePopup.DrawPopup(ref m_space, m_context, GUILayout.Width(120));

            float width = maxWidth - 120;
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
                GUILayout.Label("Space:", GUILayout.Width(60));
                isDirty |= GoogleUIUtils.ChatSpacePopup.DrawPopup(ref m_space, m_context, GUILayout.Width(200));
            }

            var messageNameFormat = new GUIContent("Message Name Format", messageNameFormatTooltip);
            GUILayout.Label(messageNameFormat, GUILayout.Width(200));
            isDirty |= ContextGUI.DrawKey(m_responseMessageNameFormat, ref m_showFormattedMessageNameFormat, m_context);

            GUILayout.Label("Text:", GUILayout.Width(50));
            if (EditorUtils.FormatStringTextArea(ref m_text, ref m_showFormattedText, m_context))
            {
                isDirty = true;
            }
        }
    }
}
