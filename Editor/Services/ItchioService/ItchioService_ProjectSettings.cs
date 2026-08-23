using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class ItchioService
    {
        public override bool HasProjectSettingsGUI => true;
        
        private ItchioUser m_currentUser;
        private Context m_context = new Context();

        private ReorderableListOfGames m_gameList = new ReorderableListOfGames();
        private ReorderableListOfChannels m_channelList;
        
        
        public override void ProjectSettingsGUI()
        {
            base.ProjectSettingsGUI();
            using (new GUILayout.VerticalScope("box"))
            {
                DrawUserDropdown();

                if (m_currentUser != null)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        DrawUser(false);
                    }
                
                    using (new GUILayout.VerticalScope())
                    {
                        DrawUserGames();
                    }
                }
                
                GUILayout.Space(10);

                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://itch.io/docs/butler/pushing.html#channel-names");
                }

                // Draw Channels
                DrawChannels();

            }
        }
        
        private void DrawUserDropdown()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Users:", GUILayout.Width(100));

                if (ItchioUIUtils.UserPopup.DrawPopup(ref m_currentUser, m_context))
                {
                    m_gameList.Initialize(m_currentUser.GameIds, "Games", true, _ => { Save(); });
                }

                if (GUILayout.Button("New", GUILayout.Width(100)))
                {
                    ItchioUser config = new ItchioUser();
                    List<ItchioUser> configs = ItchioUIUtils.GetItchioBuildData().Users;
                    config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                    configs.Add(config);
                    ItchioUIUtils.Save();
                    ItchioUIUtils.UserPopup.Refresh();
                    ItchioUIUtils.GamePopup.Refresh();
                    m_currentUser = config;
                    m_gameList.Initialize(m_currentUser.GameIds, "Games", true, _ => { Save(); });
                }

                if (m_currentUser != null)
                {
                    if (GUILayout.Button("User Profile", GUILayout.Width(200)))
                    {
                        Application.OpenURL($"https://itch.io/profile/{m_currentUser.Name}");
                    }
                }

                if (CustomSettingsIcon.OnGUI())
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Delete User"), false, ()=>
                    {
                        if (m_currentUser != null)
                        {
                            if (EditorUtility.DisplayDialog("Delete User", 
                                    "Are you sure you want to delete this user?", "Delete", "No"))
                            {
                                List<ItchioUser> configs = ItchioUIUtils.GetItchioBuildData().Users;
                                configs.Remove(m_currentUser);
                                m_currentUser = null;
                                ItchioUIUtils.Save();
                                ItchioUIUtils.UserPopup.Refresh();
                                ItchioUIUtils.GamePopup.Refresh();
                            }
                        }
                    });
                    menu.ShowAsContext();
                }
            }
        }

        private void DrawChannels()
        {
            if(m_channelList == null)
            {
                m_channelList = new ReorderableListOfChannels();
                m_channelList.Initialize(ItchioUIUtils.GetItchioBuildData().Channels, "Channels", 
                    true, _ => { Save(); });
            }
            
            if (m_channelList.OnGUI())
            {
                Save();
            }
        }

        public void DrawUser(bool preferences)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent tooltip = new GUIContent("User Name:", "The ID of your user name. (e.g. https://jamesgamesbro.itch.io/my-game. use: 'jamesgamesbro').");
                GUILayout.Label(tooltip, GUILayout.Width(100));
                string newConfigName = EditorGUILayout.TextField(m_currentUser.Name);
                if (newConfigName != m_currentUser.Name)
                {
                    m_currentUser.Name = newConfigName;
                    Save();
                    ItchioUIUtils.UserPopup.Refresh();
                    ItchioUIUtils.GamePopup.Refresh();
                }
            }

            if (preferences)
            {
                using (new GUILayout.HorizontalScope())
                {
                    string newAPIKey = PasswordField.Draw("API KEY:", "The API Key to authenticate", 105, m_currentUser.APIKey, labelIsRedIfEmpty:false);
                    if (newAPIKey != m_currentUser.APIKey)
                    {
                        m_currentUser.APIKey = newAPIKey;
                        Save();
                        ItchioUIUtils.UserPopup.Refresh();
                        ItchioUIUtils.GamePopup.Refresh();
                    }

                    if (GUILayout.Button("api-keys", GUILayout.Width(100)))
                    {
                        Application.OpenURL("https://itch.io/user/settings/api-keys");
                    }
                }
            }
        }
        
        public void DrawUserGames()
        {
            if (m_gameList.OnGUI())
            {
                Save();
                ItchioUIUtils.UserPopup.Refresh();
                ItchioUIUtils.GamePopup.Refresh();
            }
        }

        public void Save()
        {
            ItchioUIUtils.Save();
            ItchioUIUtils.UserPopup.Refresh();
            ItchioUIUtils.GamePopup.Refresh();
        }
    }
}