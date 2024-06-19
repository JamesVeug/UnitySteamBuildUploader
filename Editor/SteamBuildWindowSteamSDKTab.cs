using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindowSteamSDKTab : SteamBuildWindowTab
    {
        private SteamBuildConfig currentConfig;
        private GUIStyle m_titleStyle;

        private ReorderableListOfStrings m_branchesList = new ReorderableListOfStrings();
        private ReorderableListOfDepots m_depotsList = new ReorderableListOfDepots();

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        public override void OnGUI()
        {
            Setup();

            // Content Path
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Settings", m_titleStyle);
                using (new GUILayout.HorizontalScope())
                {
                    Color temp = GUI.color;
                    GUI.color = SteamSDK.Instance.IsInitialized ? Color.green : Color.red;
                    GUILayout.Label("SteamSDKPath:", GUILayout.Width(105));
                    GUI.color = temp;
                    
                    
                    if (!SteamSDK.Instance.IsInitialized)
                    {
                        if (GUILayout.Button("?", GUILayout.Width(20)))
                        {
                            Application.OpenURL("https://partner.steamgames.com/doc/sdk");
                        }
                    }
                    
                    string newPath = GUILayout.TextField(SteamSDK.Instance.SteamSDKPath);

                    if (GUILayout.Button("...", GUILayout.Width(50)))
                    {
                        newPath = EditorUtility.OpenFolderPanel("SteamSDK Folder", ".", "");
                    }

                    if (GUILayout.Button("Show", GUILayout.Width(50)))
                    {
                        EditorUtility.RevealInFinder(SteamSDK.Instance.SteamSDKPath);
                    }

                    if (GUILayout.Button("CMD", GUILayout.Width(50)))
                    {
                        SteamSDK.Instance.ShowConsole();
                    }

                    if (newPath != SteamSDK.Instance.SteamSDKPath && !string.IsNullOrEmpty(newPath))
                    {
                        SteamSDK.Instance.SteamSDKPath = newPath;
                        SteamSDK.Instance.Initialize();
                    }
                }

                // Steam username
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.Instance.UserName = PasswordField.Draw("Steam Username:", 105, SteamSDK.Instance.UserName);
                }

                // Steam password
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.Instance.UserPassword = PasswordField.Draw("Steam password:", 105, SteamSDK.Instance.UserPassword);
                }
            }

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Configuration", m_titleStyle);

                // Current Config
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Config:", GUILayout.Width(100));

                    if (SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref currentConfig))
                    {
                        m_branchesList.Initialize(currentConfig.Branches, "Branches");
                        m_depotsList.Initialize(currentConfig.Depots, "Depots", d=>currentConfig.Depots.Add(d));
                    }

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        SteamBuildConfig config = new SteamBuildConfig();
                        SteamBuildWindowUtil.GetSteamBuildData().Configs.Add(config);
                        SteamBuildWindowUtil.Save();
                        SteamBuildWindowUtil.ConfigPopup.Refresh();
                        currentConfig = config;
                    }

                    if (GUILayout.Button("Browse Builds", GUILayout.Width(200)))
                    {
                        if (currentConfig != null)
                        {
                            Application.OpenURL("https://partner.steamgames.com/apps/builds/" +
                                                currentConfig.App.appid);
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

                if (GUILayout.Button("Save"))
                {
                    Save();
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
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App ID:", GUILayout.Width(150));
                int newAppId = EditorGUILayout.IntField(currentConfig.App.appid);
                if (newAppId != currentConfig.App.appid)
                {
                    currentConfig.App.appid = newAppId;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Description:", GUILayout.Width(150));
                string description = EditorGUILayout.TextField(currentConfig.App.desc);
                if (description != currentConfig.App.desc)
                {
                    currentConfig.App.desc = description;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Default Branch:", GUILayout.Width(150));
                string newBranch = currentConfig.App.setlive;
                if (SteamBuildWindowUtil.BranchPopup.DrawPopup(currentConfig, ref newBranch))
                {
                    currentConfig.App.setlive = newBranch;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Preview Build:", GUILayout.Width(150));
                bool previewBuild = EditorGUILayout.Toggle(currentConfig.App.preview);
                if (previewBuild != currentConfig.App.preview)
                {
                    currentConfig.App.preview = previewBuild;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Local Content Server:", GUILayout.Width(150));
                string serverContent = EditorGUILayout.TextField(currentConfig.App.local);
                if (serverContent != currentConfig.App.local)
                {
                    currentConfig.App.local = serverContent;
                    window.QueueSave();
                }
            }
        }

        public void DrawBranches()
        {
            if (m_branchesList.OnGUI())
            {
                window.QueueSave();
                SteamBuildWindowUtil.BranchPopup.Refresh();
                Debug.Log("Branches updated");
            }
        }

        public void DrawDepots()
        {
            if (m_depotsList.OnGUI())
            {
                window.QueueSave();
                SteamBuildWindowUtil.DepotPopup.Refresh();
                Debug.Log("Depots updated");
            }
        }

        public override void Save()
        {
            SteamBuildWindowUtil.Save();
            SteamBuildWindowUtil.BranchPopup.Refresh();
        }
    }
}