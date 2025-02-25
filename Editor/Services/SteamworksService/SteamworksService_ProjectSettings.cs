using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class SteamworksService
    {
        private SteamApp _current;

        private ReorderableListOfBranches m_branchesList = new ReorderableListOfBranches();
        private ReorderableListOfDepots m_depotsList = new ReorderableListOfDepots();
        
        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Steamworks", EditorStyles.boldLabel);

                // Current Config
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Config:", GUILayout.Width(100));

                    if (SteamUIUtils.ConfigPopup.DrawPopup(ref _current))
                    {
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", _ => { Save(); });
                        m_depotsList.Initialize(_current.Depots, "Depots", _ => { Save(); });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        SteamApp config = new SteamApp();
                        List<SteamApp> configs = SteamUIUtils.GetSteamBuildData().Configs;
                        config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                        configs.Add(config);
                        SteamUIUtils.Save();
                        SteamUIUtils.ConfigPopup.Refresh();
                        _current = config;
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", _ => { Save(); });
                        m_depotsList.Initialize(_current.Depots, "Depots", _ => { Save(); });
                    }

                    if (_current != null)
                    {
                        if (GUILayout.Button("Store Page", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://store.steampowered.com/app/" + _current.App.appid);
                        }

                        if (GUILayout.Button("Browse Builds", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://partner.steamgames.com/apps/builds/" + _current.App.appid);
                        }
                    }
                }

                if (_current == null)
                {
                    return;
                }

                // Draw
                using (new GUILayout.VerticalScope())
                {
                    DrawAppData();
                }

                using (new GUILayout.VerticalScope())
                {
                    DrawDepots();
                }

                using (new GUILayout.VerticalScope())
                {
                    DrawBranches();
                }
            }
        }

        public void DrawAppData()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Name:", GUILayout.Width(150));
                string newConfigName = EditorGUILayout.TextField(_current.Name);
                if (newConfigName != _current.Name)
                {
                    _current.Name = newConfigName;
                    Save();
                    SteamUIUtils.ConfigPopup.Refresh();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App ID:", GUILayout.Width(150));
                int newAppId = EditorGUILayout.IntField(_current.App.appid);
                if (newAppId != _current.App.appid)
                {
                    _current.App.appid = newAppId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Branch:", GUILayout.Width(150));
                string newBranch = _current.App.setlive;
                var chosenBranch = _current.ConfigBranches.FirstOrDefault(b => b.name == newBranch);
                if (SteamUIUtils.BranchPopup.DrawPopup(_current, ref chosenBranch))
                {
                    _current.App.setlive = chosenBranch?.name;
                    Save();
                }
            }
        }

        public void DrawBranches()
        {
            if (m_branchesList.OnGUI())
            {
                Save();
                SteamUIUtils.BranchPopup.Refresh();
            }
        }

        public void DrawDepots()
        {
            if (m_depotsList.OnGUI())
            {
                Save();
                SteamUIUtils.DepotPopup.Refresh();
            }
        }

        public void Save()
        {
            SteamUIUtils.Save();
            SteamUIUtils.BranchPopup.Refresh();
        }
    }
}