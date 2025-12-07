using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public partial class ItchioDestination
    {
        private bool m_showFormattedDescription = Preferences.DefaultShowFormattedTextToggle;
        private bool m_queuedDirty; // Workaround for changing channels via GenericMenu since it can't reference isDirty

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user, m_context);
            isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game, m_context);

            var allChannels = ItchioUIUtils.GetItchioBuildData().Channels;
            EditorUtils.DrawPopup(m_channels, allChannels, "Choose Channels",
                (newChannels) =>
                {
                    m_channels = newChannels;
                    m_queuedDirty = true;
                });

            isDirty |= m_queuedDirty;
            m_queuedDirty = false;
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                Application.OpenURL("https://itch.io/docs/butler/pushing.html");
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("User:", GUILayout.Width(120));
                isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Game:", GUILayout.Width(120));
                isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game, m_context);
            }

            GUILayout.Label("Channels:", GUILayout.Width(120));
            DrawChannels(ref isDirty);
            
            

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Description Format:", "Description for developers that appears on Steamworks.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextArea(ref m_descriptionFormat, ref m_showFormattedDescription, m_context);
            }

            isDirty |= m_queuedDirty;
            m_queuedDirty = false;
        }

        private void DrawChannels(ref bool isDirty)
        {
            List<ItchioChannel> allChannels = ItchioUIUtils.GetItchioBuildData().Channels;
            int v = m_channels.RemoveAll(channel => !allChannels.Contains(channel));
            if(v > 0)
            {
                isDirty = true;
            }
            
            foreach (ItchioChannel channel in allChannels)
            {
                using (new GUILayout.HorizontalScope())
                {
                    bool active = m_channels.Contains(channel);
                    bool newActive = GUILayout.Toggle(active, channel.DisplayName, GUILayout.Width(100));
                    if (newActive != active)
                    {
                        isDirty = true;
                        if (newActive)
                        {
                            if (!m_channels.Contains(channel))
                            {
                                m_channels.Add(channel);
                            }
                        }
                        else
                        {
                            m_channels.Remove(channel);
                        }
                    }
                }
            }
        }
    }
}