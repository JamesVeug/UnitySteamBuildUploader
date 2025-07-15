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
            string user = StringFormatter.FormatString(m_user);
            string game = StringFormatter.FormatString(m_game);
            string target = "";
            if(m_channels == null || m_channels.Count == 0)
            {
                target = "???";
            }
            else
            {
                target = m_channels.ConvertAll(StringFormatter.FormatString)
                    .Aggregate((current, next) => $"{current}-{next}");
            }
            
            
            // https://jamesgamesbro.itch.io/builduploadertest-windows-mac v1.2.3
            string text = $"{user}/{game}:{target}";
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
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
                isDirty |= EditorUtils.FormatStringTextField(ref m_user, ref m_showFormattedUser);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Game:", GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_game, ref m_showFormattedGame);
            }

            GUILayout.Label("Channels:", GUILayout.Width(120));
            DrawChannels(ref isDirty);
        }

        private void DrawChannels(ref bool isDirty)
        {
            string[] allChannels = { "windows", "mac", "linux", "android" };
            int v = m_channels.RemoveAll(channel => !allChannels.Contains(channel));
            if(v > 0)
            {
                isDirty = true;
            }
            
            foreach (string channel in allChannels)
            {
                using (new GUILayout.HorizontalScope())
                {
                    bool active = m_channels.Contains(channel);
                    bool newActive = GUILayout.Toggle(active, channel, GUILayout.Width(100));
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
                        m_channels.Sort();
                    }
                }
            }
        }
    }
}