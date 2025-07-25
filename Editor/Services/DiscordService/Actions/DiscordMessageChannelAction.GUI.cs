using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class DiscordMessageChannelAction
    {
        private bool m_showFormattedText;
        private ReorderableListOfDiscordMessageEmbeds m_embedList;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= DiscordUIUtils.AppPopup.DrawPopup(ref m_app, GUILayout.Width(120));
            isDirty |= DiscordUIUtils.ServerPopup.DrawPopup(ref m_server, GUILayout.Width(120));
            isDirty |= DiscordUIUtils.ChannelPopup.DrawPopup(m_server, ref m_channel, GUILayout.Width(120));

            float width = maxWidth - 375;
            int newLine = m_text.IndexOf('\n');
            if (newLine >= 0)
            {
                m_text = m_text.Substring(0, newLine);
            }
            string truncated = Utils.TruncateText(m_text, width, "");
            using (new EditorGUI.DisabledScope(true))
            {
                var newText = GUILayout.TextArea(truncated, GUILayout.Width(width));
                if (newText != m_text)
                {
                    m_text = newText;
                    isDirty = true;
                }
            }
        }

        public override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= DiscordUIUtils.AppPopup.DrawPopup(ref m_app, GUILayout.Width(120));
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Server:", GUILayout.Width(120));
                isDirty |= DiscordUIUtils.ServerPopup.DrawPopup(ref m_server, GUILayout.Width(120));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Channel:", GUILayout.Width(120));
                // Draw the channel popup
                if (m_server == null)
                {
                    GUILayout.Label("No server selected", GUILayout.Width(120));
                }
                else
                {
                    isDirty |= DiscordUIUtils.ChannelPopup.DrawPopup(m_server, ref m_channel, GUILayout.Width(120));
                }
            }

            
            GUILayout.Label("Text:", GUILayout.Width(50));
            if(EditorUtils.FormatStringTextArea(ref m_text, ref m_showFormattedText, ctx))
            {
                isDirty = true;
            }

            if (m_embedList == null)
            {
                m_embedList = new ReorderableListOfDiscordMessageEmbeds();
                m_embedList.Initialize(m_embeds, "Embeds", (_) =>
                {
                    
                });
            }

            if (m_embedList.OnGUI())
            {
                isDirty = true;
            }
            
            
        }
    }
}