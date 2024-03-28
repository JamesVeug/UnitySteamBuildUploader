using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindowSteamSDKTab : SteamBuildWindowTab
    {
        private SteamBuildConfig currentConfig;
        private GUIStyle m_titleStyle;

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
                    GUILayout.Label("SteamSDKPath", GUILayout.Width(100));
                    GUI.color = temp;
                    string newPath = GUILayout.TextField(SteamSDK.Instance.SteamSDKPath);

                    if (GUILayout.Button("...", GUILayout.Width(50)))
                    {
                        newPath = EditorUtility.OpenFolderPanel("SteamSDK Folder", ".", "");
                    }

                    if (GUILayout.Button("Show", GUILayout.Width(50)))
                    {
                        EditorUtility.RevealInFinder(SteamSDK.Instance.SteamSDKPath);
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
                    GUILayout.Label("Steam Username:", GUILayout.Width(100));
                    SteamSDK.Instance.UserName = GUILayout.TextField(SteamSDK.Instance.UserName);
                }

                // Steam password
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Steam password:", GUILayout.Width(100));
                    SteamSDK.Instance.UserPassword = DrawPassword(SteamSDK.Instance.UserPassword);
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

                    SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref currentConfig);

                    if (GUILayout.Button("New", GUILayout.Width(100)))
                    {
                        SteamBuildWindowUtil.GetSteamBuildData().Configs.Add(new SteamBuildConfig());
                        SteamBuildWindowUtil.Save();
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
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branches:", GUILayout.Width(150));
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    currentConfig.Branches.Add("example_name");
                    window.QueueSave();
                }

                using (new GUILayout.VerticalScope())
                {
                    for (int i = 0; i < currentConfig.Branches.Count; i++)
                    {
                        if (i == 0)
                        {
                            // None specifies the build will not auto upload to this branch
                            currentConfig.Branches[i] = "none";
                            GUILayout.Label("none");
                        }
                        else
                        {
                            string newBranchName = GUILayout.TextField(currentConfig.Branches[i], GUILayout.Width(100));
                            if (newBranchName != currentConfig.App.local)
                            {
                                currentConfig.Branches[i] = newBranchName;
                                window.QueueSave();
                                SteamBuildWindowUtil.BranchPopup.Refresh();
                            }
                        }
                    }
                }
            }
        }

        public void DrawDepots()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depots:", GUILayout.Width(150));
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    SteamBuildDepot depot = new SteamBuildDepot();
                    depot.Depot.DepotID = 999999;
                    currentConfig.Depots.Add(depot);
                    window.QueueSave();
                }

                using (new GUILayout.VerticalScope())
                {
                    for (int i = 0; i < currentConfig.Depots.Count; i++)
                    {
                        DrawDepot(currentConfig.Depots[i]);
                    }
                }
            }
        }

        private void DrawDepot(SteamBuildDepot depot)
        {
            using (new GUILayout.HorizontalScope())
            {
                depot.Name = GUILayout.TextField(depot.Name, GUILayout.Width(100));

                int depotId = EditorGUILayout.IntField(depot.Depot.DepotID, GUILayout.Width(150));
                if (depotId != depot.Depot.DepotID)
                {
                    depot.Depot.DepotID = depotId;
                    window.QueueSave();
                }
            }
        }

        public override void Save()
        {
            SteamBuildWindowUtil.Save();
            SteamBuildWindowUtil.BranchPopup.Refresh();
        }

        private string DrawPassword(string password)
        {
            if (password == null)
            {
                password = "";
            }

            string newPassword = GUILayout.PasswordField(password, '*');
            return newPassword;
        }
    }
}