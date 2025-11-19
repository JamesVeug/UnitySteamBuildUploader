using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class SlackService
    {
        private static ReorderableListOfSlackAppsProjectSettings _reorderableListOfSlackAppsProjectSettings;
        private static ReorderableListOfSlackChannels _reorderableListOfSlackChannels;
        private static SlackConfig.SlackServer m_SelectedServer;
        private static StringFormatter.Context m_context = new StringFormatter.Context();

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                SlackConfig SlackConfig = SlackUIUtils.GetConfig();

                GUILayout.Label("Servers", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (SlackUIUtils.ServerPopup.DrawPopup(ref m_SelectedServer, m_context, GUILayout.Width(120)))
                    {
                        _reorderableListOfSlackChannels = null;
                    }
                    
                    if(GUILayout.Button("Add Server", GUILayout.Width(100)))
                    {
                        SlackConfig.SlackServer config = new SlackConfig.SlackServer();
                        List<SlackConfig.SlackServer> servers = SlackUIUtils.GetConfig().servers;
                        config.Id = servers.Count > 0 ? servers.Max(a=>a.Id) + 1 : 1;
                        servers.Add(config);
                        SlackUIUtils.Save();
                        SlackUIUtils.ServerPopup.Refresh();
                        SlackUIUtils.AppPopup.Refresh();
                        SlackUIUtils.ChannelPopup.Refresh();
                        _reorderableListOfSlackChannels = null;
                        m_SelectedServer = config;
                    }
                    
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledGroupScope(m_SelectedServer == null))
                    {
                        if (GUILayout.Button("Remove Server", GUILayout.Width(100)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Server",
                                    "Are you sure you want to remove the selected Slack server?", "Yes", "No"))
                            {
                                List<SlackConfig.SlackServer> servers = SlackUIUtils.GetConfig().servers;
                                servers.Remove(m_SelectedServer);
                                SlackUIUtils.Save();
                                SlackUIUtils.ServerPopup.Refresh();
                                SlackUIUtils.AppPopup.Refresh();
                                SlackUIUtils.ChannelPopup.Refresh();
                                m_SelectedServer = null;
                            }
                        }
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
                                SlackUIUtils.Save();
                                SlackUIUtils.ServerPopup.Refresh();
                                SlackUIUtils.AppPopup.Refresh();
                                SlackUIUtils.ChannelPopup.Refresh();
                            }
                        }
                        
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Server ID:", GUILayout.Width(120));
                            int newConfigName = EditorGUILayout.IntField(m_SelectedServer.ServerID);
                            if (newConfigName != m_SelectedServer.ServerID)
                            {
                                m_SelectedServer.ServerID = newConfigName;
                                SlackUIUtils.Save();
                                SlackUIUtils.ServerPopup.Refresh();
                                SlackUIUtils.AppPopup.Refresh();
                                SlackUIUtils.ChannelPopup.Refresh();
                            }
                        }
                        
                        if(_reorderableListOfSlackChannels == null)
                        {
                            _reorderableListOfSlackChannels = new ReorderableListOfSlackChannels();
                            _reorderableListOfSlackChannels.Initialize(m_SelectedServer.channels, "Channels", 
                                true, (_) => { SlackUIUtils.Save(); });
                        }
                        
                        if (_reorderableListOfSlackChannels.OnGUI())
                        {
                            SlackUIUtils.Save();
                        }
                    }
                }


                GUILayout.Space(20);

                if (_reorderableListOfSlackAppsProjectSettings == null)
                {
                    _reorderableListOfSlackAppsProjectSettings = new ReorderableListOfSlackAppsProjectSettings();
                    _reorderableListOfSlackAppsProjectSettings.Initialize(SlackConfig.apps, "Apps",
                        true, (_) => { SlackUIUtils.Save(); });
                }

                GUILayout.Label("Apps", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Apps are created on the developer dashboard.");
                    if (GUILayout.Button("Developer Dashboard", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://Slack.com/developers/applications");
                    }
                }
                GUILayout.Label("See Edit->Preferences->Build Uploader->Services->Slack to enter App Token");
                
                if (_reorderableListOfSlackAppsProjectSettings.OnGUI())
                {
                    SlackUIUtils.Save();
                }

                GUILayout.Label("To set Token used by the App/Bos see Preferences->Build Uploader->Services->Slack.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}