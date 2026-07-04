using UnityEngine;

namespace Wireframe
{
    public partial class DebugLogAction
    {
        private bool m_showFormattedMessage = Preferences.DefaultShowFormattedTextToggle;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            bool alwaysFormatted = true;
            EditorUtils.FormatStringTextArea(ref m_message, ref alwaysFormatted, m_context, null, GUILayout.Width(maxWidth));
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            GUILayout.Label("Message:");
            if (EditorUtils.FormatStringTextArea(ref m_message, ref m_showFormattedMessage, m_context))
            {
                isDirty = true;
            }
        }
    }
}
