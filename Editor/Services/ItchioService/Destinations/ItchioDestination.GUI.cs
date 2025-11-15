using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class ItchioDestination
    {
        private bool m_queuedDirty; // Workaround for changing channels via GenericMenu since it can't reference isDirty

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user, ctx);
            isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game, ctx);

            string channels = m_channels.Count > 0
                ? string.Join(", ", m_channels.Select(c => c.DisplayName))
                : "Choose Channels";
            
            if (GUILayout.Button(channels)) 
            {
                GenericMenu menu = new GenericMenu();
                foreach (ItchioChannel channel in ItchioUIUtils.GetItchioBuildData().Channels.OrderBy(a=>a.DisplayName))
                {
                    bool isSelected = m_channels.Contains(channel);
                    menu.AddItem(new GUIContent(channel.DisplayName), isSelected, () =>
                    {
                        if (isSelected)
                        {
                            m_channels.Remove(channel);
                        }
                        else
                        {
                            m_channels.Add(channel);
                            m_channels.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
                        }

                        m_queuedDirty = true;
                    });
                }
                menu.ShowAsContext();
            }

            isDirty |= m_queuedDirty;
            m_queuedDirty = false;
        }

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                Application.OpenURL("https://itch.io/docs/butler/pushing.html");
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("User:", GUILayout.Width(120));
                isDirty |= ItchioUIUtils.UserPopup.DrawPopup(ref m_user, ctx);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Game:", GUILayout.Width(120));
                isDirty |= ItchioUIUtils.GamePopup.DrawPopup(m_user, ref m_game, ctx);
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