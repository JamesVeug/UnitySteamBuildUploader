using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Wireframe
{
    internal class WindowUploadTab : WindowTab
    {
        private static readonly string FilePath = Application.persistentDataPath + "/BuildUploader/WindowUploadTab.json";

        [Serializable]
        internal class UploadTabData
        {
            [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        }

        public override string TabName => "Upload";
        
        private List<BuildConfig> m_buildsToUpload;

        private GUIStyle m_titleStyle;
        private Vector2 m_scrollPosition;
        private string m_buildDescription;
        private bool m_isDirty;

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            if (m_buildsToUpload == null)
                Load();
        }

        public override void OnGUI()
        {
            Setup();

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label("Builds to Upload", m_titleStyle);
                DrawSaveButton();

                if (GUILayout.Button("New"))
                {
                    BuildConfig buildConfigSetup = new BuildConfig((BuildUploaderWindow)UploaderWindow);
                    buildConfigSetup.Collapsed = true;
                    m_buildsToUpload.Add(buildConfigSetup);
                    m_isDirty = true;
                }

                // Builds to upload
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (int i = 0; i < m_buildsToUpload.Count; i++)
                {
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Build",
                                    "Are you sure you want to remove this build config?", "Yes"))
                            {
                                m_buildsToUpload.RemoveAt(i--);
                                m_isDirty = true;
                                continue;
                            }
                        }

                        BuildConfig buildConfig = m_buildsToUpload[i];
                        bool e = EditorGUILayout.Toggle(buildConfig.Enabled, GUILayout.Width(20));
                        if (e != buildConfig.Enabled)
                        {
                            buildConfig.Enabled = e;
                            m_isDirty = true;
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            buildConfig.OnGUI(ref m_isDirty, UploaderWindow);
                        }

                        bool collapse = buildConfig.Collapsed;
                        if (GUILayout.Button(collapse ? ">" : "\\/", GUILayout.Width(20)))
                        {
                            buildConfig.Collapsed = !buildConfig.Collapsed;
                        }
                    }
                }

                EditorGUILayout.EndScrollView();


                GUILayout.FlexibleSpace();

                // Description
                GUILayout.Label("Build", m_titleStyle);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Description:", GUILayout.Width(100));
                    m_buildDescription = GUILayout.TextField(m_buildDescription);
                }

                // Upload all
                bool startButtonDisabled = !CanStartBuild(out string reason);
                using (new EditorGUI.DisabledScope(startButtonDisabled))
                {
                    string text = startButtonDisabled ? ("Cannot continue: \nReason: " + reason) : "Download and Upload all";
                    if (GUILayout.Button(text, GUILayout.Height(100)))
                    {
                        if (EditorUtility.DisplayDialog("Download and Upload all",
                                "Are you sure you want to upload all enabled builds?",
                                "Yes", "Cancel"))
                        {
                            DownloadAndUpload();
                        }
                    }
                }
            }
        }

        private void DrawSaveButton()
        {
            string text = m_isDirty ? "Save*" : "Save";
            if (GUILayout.Button(text))
            {
                Save();
            }
        }

        private async Task DownloadAndUpload()
        {
            // Start uploading
            BuildTask buildTask = new (m_buildsToUpload, m_buildDescription);
            await buildTask.Start(()=> UploaderWindow.Repaint());
            UploaderWindow.Repaint();
        }

        private bool CanStartBuild(out string reason)
        {
            if (string.IsNullOrEmpty(m_buildDescription))
            {
                reason = "No Description";
                return false;
            }

            if (m_buildsToUpload == null)
            {
                reason = "No builds to upload!";
                return false;
            }

            int validBuilds = 0;
            for (int i = 0; i < m_buildsToUpload.Count; i++)
            {
                if (!m_buildsToUpload[i].Enabled)
                    continue;

                if (!m_buildsToUpload[i].CanStartBuild(out string buildReason))
                {
                    reason = $"Build {i+1}: {buildReason}";
                    return false;
                }

                validBuilds++;
            }

            // Make sure there is at least 1 build to build
            bool canStartBuild = validBuilds > 0;
            reason = !canStartBuild ? "No builds set up!" : "";
            
            return canStartBuild;
        }

        public override void Save()
        {
            m_isDirty = false;
            if (m_buildsToUpload == null)
            {
                m_buildsToUpload = new List<BuildConfig>();
            }

            UploadTabData data = new UploadTabData();
            for (int i = 0; i < m_buildsToUpload.Count; i++)
            {
                data.Data.Add(m_buildsToUpload[i].Serialize());
            }

            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JSON.SerializeObject(data);
            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Close();
            }

            File.WriteAllText(FilePath, json);
        }

        public void Load()
        {
            if (File.Exists(FilePath))
            {
                LoadFromPath(FilePath);
            }
            else if (File.Exists(Application.persistentDataPath + "/SteamBuilder/WindowUploadTab.json"))
            {
                Debug.Log("SteamBuildData exists from a previous version. Migrating it over");
                LoadFromPath(Application.persistentDataPath + "/SteamBuilder/WindowUploadTab.json");
                Save();
            }
            else
            {
                Debug.Log("SteamBuildData does not exist. Creating new file");
                m_buildsToUpload = new List<BuildConfig>();
                Save();
            }
        }

        private void LoadFromPath(string filePath)
        {
            string json = File.ReadAllText(filePath);
            UploadTabData config = JSON.DeserializeObject<UploadTabData>(json);
            if (config == null)
            {
                Debug.Log("Config is null. Creating new config");
                m_buildsToUpload = new List<BuildConfig>();
                Save();
            }
            else
            {
                m_buildsToUpload = new List<BuildConfig>();
                for (int i = 0; i < config.Data.Count; i++)
                {
                    try
                    {
                        BuildConfig buildConfig = new BuildConfig(UploaderWindow);
                        buildConfig.Collapsed = true;
                        var jObject = config.Data[i];
                        buildConfig.Deserialize(jObject);
                        m_buildsToUpload.Add(buildConfig);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load build config: #" + (i+1));
                        Debug.LogException(e);
                        BuildConfig buildConfig = new BuildConfig(UploaderWindow);
                        buildConfig.Collapsed = true;
                        m_buildsToUpload.Add(buildConfig);
                    }
                }
            }
        }
    }
}