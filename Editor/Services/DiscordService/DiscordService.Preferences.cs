using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class DiscordService
    {
        private static ReorderableListOfDiscordAppsPreferences _reorderableListOfDiscordAppsPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Apps are created on the developer dashboard.");
                if (GUILayout.Button("Developer Dashboard", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://discord.com/developers/applications");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Discord.Enabled = GUILayout.Toggle(Discord.Enabled, "Enabled");
                if (!Discord.Enabled)
                {
                    return;
                }

                DiscordConfig discordConfig = DiscordUIUtils.GetConfig();
                if (_reorderableListOfDiscordAppsPreferences == null)
                {
                    _reorderableListOfDiscordAppsPreferences = new ReorderableListOfDiscordAppsPreferences();
                    _reorderableListOfDiscordAppsPreferences.Initialize(discordConfig.apps, "Apps", 
                        true, (_) => 
                        {
                            DiscordUIUtils.AppPopup.Refresh();
                            DiscordUIUtils.ServerPopup.Refresh();
                            DiscordUIUtils.ChannelPopup.Refresh();
                            DiscordUIUtils.Save();
                        });
                }

                if (_reorderableListOfDiscordAppsPreferences.OnGUI())
                {
                    DiscordUIUtils.AppPopup.Refresh();
                    DiscordUIUtils.ServerPopup.Refresh();
                    DiscordUIUtils.ChannelPopup.Refresh();
                    DiscordUIUtils.Save();
                }
            }
        }
    }
}