using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class ProjectSettings : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new ProjectSettings("Project/BuildUploader", SettingsScope.Project)
                {
                    label = "Build Uploader",
                    keywords = new HashSet<string>(new[] { "Steam", "Build", "Upload", "Pipe", "line" })
                };

            return provider;
        }


        private SteamApp _current;
        private GUIStyle m_titleStyle;

        private ReorderableListOfBranches m_branchesList = new ReorderableListOfBranches();
        private ReorderableListOfDepots m_depotsList = new ReorderableListOfDepots();

        private ProjectSettings(string path, SettingsScope scopes,
            IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            if (m_titleStyle == null)
            {
                Setup();
            }

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Configuration", m_titleStyle);

                // Current Config
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Config:", GUILayout.Width(100));

                    if (UIUtils.ConfigPopup.DrawPopup(ref _current))
                    {
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", _ =>
                        {
                            Save();
                        });
                        m_depotsList.Initialize(_current.Depots, "Depots", _ =>
                        {
                            Save();
                        });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        SteamApp config = new SteamApp();
                        List<SteamApp> configs = UIUtils.GetSteamBuildData().Configs;
                        config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                        configs.Add(config);
                        UIUtils.Save();
                        UIUtils.ConfigPopup.Refresh();
                        _current = config;
                        m_branchesList.Initialize(_current.ConfigBranches, "Branches", _ =>
                        {
                            Save();
                        });
                        m_depotsList.Initialize(_current.Depots, "Depots", _ =>
                        {
                            Save();
                        });
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
                    UIUtils.ConfigPopup.Refresh();
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
                if (UIUtils.BranchPopup.DrawPopup(_current, ref chosenBranch))
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
                UIUtils.BranchPopup.Refresh();
            }
        }

        public void DrawDepots()
        {
            if (m_depotsList.OnGUI())
            {
                Save();
                UIUtils.DepotPopup.Refresh();
            }
        }

        public void Save()
        {
            UIUtils.Save();
            UIUtils.BranchPopup.Refresh();
        }
    }
}