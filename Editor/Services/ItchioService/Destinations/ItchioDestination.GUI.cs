using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class ItchioDestination
    {
        private bool m_showFormattedUser;
        private bool m_showFormattedGame;
        private bool m_showFormattedVersion;
        private bool m_showFormattedChannels;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user);
            isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game);
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
                isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Game:", GUILayout.Width(120));
                isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game);
            }

            GUILayout.Label("Channels:", GUILayout.Width(120));
            DrawChannels(ref isDirty);
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