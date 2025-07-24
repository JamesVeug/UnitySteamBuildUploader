using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class DiscordService
    {
        private static ReorderableListOfDiscordAppsProjectSettings _reorderableListOfDiscordAppsProjectSettings;
        private static ReorderableListOfDiscordChannels _reorderableListOfDiscordChannels;
        private static DiscordConfig.DiscordServer m_SelectedServer;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Discord", EditorStyles.boldLabel);
                DiscordConfig discordConfig = DiscordUIUtils.GetConfig();

                GUILayout.Label("Servers");
                using (new EditorGUILayout.HorizontalScope())
                {
                    DiscordUIUtils.ServerPopup.DrawPopup(ref m_SelectedServer, GUILayout.Width(120));
                    if(GUILayout.Button("Add Server", GUILayout.Width(100)))
                    {
                        DiscordConfig.DiscordServer config = new DiscordConfig.DiscordServer();
                        List<DiscordConfig.DiscordServer> servers = DiscordUIUtils.GetConfig().servers;
                        config.ID = servers.Count > 0 ? servers[servers.Count - 1].Id + 1 : 1;
                        servers.Add(config);
                        DiscordUIUtils.Save();
                        DiscordUIUtils.ServerPopup.Refresh();
                        m_SelectedServer = config;
                    }
                }

                using (new GUILayout.VerticalScope("box"))
                {
                    if (m_SelectedServer != null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Name:", GUILayout.Width(120));
                            string newConfigName = EditorGUILayout.TextField(m_SelectedServer.Name);
                            if (newConfigName != m_SelectedServer.Name)
                            {
                                m_SelectedServer.Name = newConfigName;
                                DiscordUIUtils.Save();
                                DiscordUIUtils.ServerPopup.Refresh();
                            }
                        }
                        
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Server ID:", GUILayout.Width(120));
                            int newConfigName = EditorGUILayout.IntField(m_SelectedServer.ServerID);
                            if (newConfigName != m_SelectedServer.ServerID)
                            {
                                m_SelectedServer.ServerID = newConfigName;
                                DiscordUIUtils.Save();
                                DiscordUIUtils.ServerPopup.Refresh();
                            }
                        }
                        
                        if(_reorderableListOfDiscordChannels == null)
                        {
                            _reorderableListOfDiscordChannels = new ReorderableListOfDiscordChannels();
                            _reorderableListOfDiscordChannels.Initialize(m_SelectedServer.channels, "Channels",
                                (_) => { DiscordUIUtils.Save(); });
                        }
                        
                        if (_reorderableListOfDiscordChannels.OnGUI())
                        {
                            DiscordUIUtils.Save();
                        }
                    }
                }


                GUILayout.Space(20);

                if (_reorderableListOfDiscordAppsProjectSettings == null)
                {
                    _reorderableListOfDiscordAppsProjectSettings = new ReorderableListOfDiscordAppsProjectSettings();
                    _reorderableListOfDiscordAppsProjectSettings.Initialize(discordConfig.apps, "Apps",
                        (_) => { DiscordUIUtils.Save(); });
                }

                if (_reorderableListOfDiscordAppsProjectSettings.OnGUI())
                {
                    DiscordUIUtils.Save();
                }

                GUILayout.Label("To set Token used by the App/Bos see Preferences->Build Uploader->Services->Discord.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}