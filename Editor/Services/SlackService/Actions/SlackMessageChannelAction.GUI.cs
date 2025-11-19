using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class SlackMessageChannelAction
    {
        private bool m_showFormattedText;
        private ReorderableListOfSlackMessageAttachments m_attachmentList;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= SlackUIUtils.AppPopup.DrawPopup(ref m_app, ctx, GUILayout.Width(120));
            isDirty |= SlackUIUtils.ServerPopup.DrawPopup(ref m_server, ctx, GUILayout.Width(120));
            isDirty |= SlackUIUtils.ChannelPopup.DrawPopup(m_server, ref m_channel, ctx, GUILayout.Width(120));

            float width = maxWidth - 375;
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
                isDirty |= SlackUIUtils.AppPopup.DrawPopup(ref m_app, ctx, GUILayout.Width(120));
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Server:", GUILayout.Width(120));
                isDirty |= SlackUIUtils.ServerPopup.DrawPopup(ref m_server, ctx, GUILayout.Width(120));
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
                    isDirty |= SlackUIUtils.ChannelPopup.DrawPopup(m_server, ref m_channel, ctx, GUILayout.Width(120));
                }
            }

            
            GUILayout.Label("Text:", GUILayout.Width(50));
            if (EditorUtils.FormatStringTextArea(ref m_text, ref m_showFormattedText, ctx))
            {
                isDirty = true;
            }

            if (m_attachmentList == null)
            {
                m_attachmentList = new ReorderableListOfSlackMessageAttachments();
                m_attachmentList.Initialize(m_attachments, "Attachments", m_attachments.Count <= 1, (_) =>
                {
                    
                });
            }

            if (m_attachmentList.OnGUI())
            {
                isDirty = true;
            }
            
            
        }
    }
}