using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GitTagCommitAction
    {
        private bool m_showFormattedTag = Preferences.DefaultShowFormattedTextToggle;
        private bool m_showFormattedRemote = Preferences.DefaultShowFormattedTextToggle;

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            string tag = m_context.FormatString(m_tagFormat);
            string remote = m_context.FormatString(m_remote);
            string text = m_push ? $"git tag {tag} (+push to {remote})" : $"git tag {tag}";
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Tag:", "The name of the tag to put on the commit the project is currently on.");
                GUILayout.Label(label, GUILayout.Width(120));
                bool tagFormatChanged = EditorUtils.FormatStringTextField(ref m_tagFormat, ref m_showFormattedTag, m_context);
                if (tagFormatChanged)
                {
                    m_tagFormat = Utils.Replace(m_tagFormat.Trim(), " ", "-", StringComparison.OrdinalIgnoreCase);
                }
                isDirty |= tagFormatChanged;
            }

            using (new GUILayout.HorizontalScope())
            {
                string remote = m_context.FormatString(m_remote);
                GUIContent label = new GUIContent("Auto Push:", $"If true, the tag is pushed to '{remote}' as soon as it is created.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_push);
            }

            using (new EditorGUI.DisabledScope(!m_push))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUIContent label = new GUIContent("Remote:", "When pushing tag to git, this is the remote that is used.");
                    GUILayout.Label(label, GUILayout.Width(120));
                    isDirty |= EditorUtils.FormatStringTextField(ref m_remote, ref m_showFormattedRemote, m_context);
                }
            }
        }
    }
}
