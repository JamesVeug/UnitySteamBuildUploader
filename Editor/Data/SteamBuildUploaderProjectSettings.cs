using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class SteamBuildUploaderSettingsIMGUIRegister : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSteamBuildUploaderSettingsProvider()
        {
            var provider =
                new SteamBuildUploaderSettingsIMGUIRegister("Project/SteamBuildUploader", SettingsScope.Project)
                {
                    label = "Steam Build Uploader",
                    keywords = new HashSet<string>(new[] { "Steam", "Build", "Upload", "Pipe", "line" })
                };

            return provider;
        }


        private SteamBuildConfig currentConfig;
        private GUIStyle m_titleStyle;

        private ReorderableListOfBranches m_branchesList = new ReorderableListOfBranches();
        private ReorderableListOfDepots m_depotsList = new ReorderableListOfDepots();

        private SteamBuildUploaderSettingsIMGUIRegister(string path, SettingsScope scopes,
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

                    if (SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref currentConfig))
                    {
                        m_branchesList.Initialize(currentConfig.ConfigBranches, "Branches", _ =>
                        {
                            Save();
                        });
                        m_depotsList.Initialize(currentConfig.Depots, "Depots", _ =>
                        {
                            Save();
                        });
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        SteamBuildConfig config = new SteamBuildConfig();
                        List<SteamBuildConfig> configs = SteamBuildWindowUtil.GetSteamBuildData().Configs;
                        config.ID = configs.Count > 0 ? configs[configs.Count - 1].Id + 1 : 1;
                        configs.Add(config);
                        SteamBuildWindowUtil.Save();
                        SteamBuildWindowUtil.ConfigPopup.Refresh();
                        currentConfig = config;
                        m_branchesList.Initialize(currentConfig.ConfigBranches, "Branches", _ =>
                        {
                            Save();
                        });
                        m_depotsList.Initialize(currentConfig.Depots, "Depots", _ =>
                        {
                            Save();
                        });
                    }

                    if (currentConfig != null)
                    {
                        if (GUILayout.Button("Store Page", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://store.steampowered.com/app/" + currentConfig.App.appid);
                        }

                        if (GUILayout.Button("Browse Builds", GUILayout.Width(200)))
                        {
                            Application.OpenURL("https://partner.steamgames.com/apps/builds/" + currentConfig.App.appid);
                        }
                    }
                }

                if (currentConfig == null)
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
                string newConfigName = EditorGUILayout.TextField(currentConfig.Name);
                if (newConfigName != currentConfig.Name)
                {
                    currentConfig.Name = newConfigName;
                    Save();
                    SteamBuildWindowUtil.ConfigPopup.Refresh();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App ID:", GUILayout.Width(150));
                int newAppId = EditorGUILayout.IntField(currentConfig.App.appid);
                if (newAppId != currentConfig.App.appid)
                {
                    currentConfig.App.appid = newAppId;
                    Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Branch:", GUILayout.Width(150));
                string newBranch = currentConfig.App.setlive;
                var chosenBranch = currentConfig.ConfigBranches.FirstOrDefault(b => b.name == newBranch);
                if (SteamBuildWindowUtil.BranchPopup.DrawPopup(currentConfig, ref chosenBranch))
                {
                    currentConfig.App.setlive = chosenBranch?.name;
                    Save();
                }
            }
        }

        public void DrawBranches()
        {
            if (m_branchesList.OnGUI())
            {
                Save();
                SteamBuildWindowUtil.BranchPopup.Refresh();
            }
        }

        public void DrawDepots()
        {
            if (m_depotsList.OnGUI())
            {
                Save();
                SteamBuildWindowUtil.DepotPopup.Refresh();
            }
        }

        public void Save()
        {
            SteamBuildWindowUtil.Save();
            SteamBuildWindowUtil.BranchPopup.Refresh();
        }
    }
}