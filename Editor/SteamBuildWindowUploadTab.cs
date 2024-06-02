using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindowUploadTab : SteamBuildWindowTab
    {
        private static readonly string FilePath = Application.persistentDataPath + "/SteamBuilder/WindowUploadTab.json";

        [Serializable]
        public class UploadTabData
        {
            [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        }

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

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Builds to Upload", m_titleStyle);
                DrawSaveButton();

                if (GUILayout.Button("New"))
                {
                    BuildConfig buildConfigSetup = new BuildConfig(window);
                    buildConfigSetup.Collapsed = true;
                    m_buildsToUpload.Add(buildConfigSetup);
                    m_isDirty = true;
                }

                // Builds to upload
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (int i = 0; i < m_buildsToUpload.Count; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("X"))
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
                            buildConfig.OnGUI(ref m_isDirty);
                        }

                        bool collapse = buildConfig.Collapsed;
                        if (GUILayout.Button(collapse ? "+" : "-"))
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
                bool startButtonDisabled = !CanStartBuild();
                using (new EditorGUI.DisabledScope(startButtonDisabled))
                {
                    if (GUILayout.Button("Download and Upload all", GUILayout.Height(100)))
                    {
                        if (EditorUtility.DisplayDialog("Download and Upload all",
                                "Are you sure you want to upload all enabled builds?",
                                "Yes", "Cancel"))
                        {
                            EditorCoroutineUtility.StartCoroutine(DownloadAndUpload(), window);
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

        private IEnumerator DownloadAndUpload()
        {
            // Start uploading
            SteamWindowBuildProgressWindow buildProgressWindow = new (m_buildsToUpload, m_buildDescription);
            IEnumerator startProgress = buildProgressWindow.StartProgress();
            yield return startProgress;
        }

        private bool CanStartBuild()
        {
            if (string.IsNullOrEmpty(m_buildDescription))
            {
                return false;
            }

            if (m_buildsToUpload == null)
            {
                return false;
            }

            int validBuilds = 0;
            for (int i = 0; i < m_buildsToUpload.Count; i++)
            {
                if (!m_buildsToUpload[i].Enabled)
                    continue;

                if (!m_buildsToUpload[i].CanStartBuild())
                {
                    return false;
                }

                validBuilds++;
            }

            // Make sure there is at least 1 build to build
            return validBuilds > 0;
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

            string json = JsonUtility.ToJson(data, true);
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
                string json = File.ReadAllText(FilePath);
                UploadTabData config = JsonUtility.FromJson<UploadTabData>(json);
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
                        BuildConfig buildConfig = new BuildConfig(window);
                        buildConfig.Collapsed = true;
                        var jObject = config.Data[i];
                        buildConfig.Deserialize(jObject);
                        m_buildsToUpload.Add(buildConfig);
                    }
                }
            }
            else
            {
                Debug.Log("SteamBuildData does not exist. Creating new file");
                m_buildsToUpload = new List<BuildConfig>();
                Save();
            }
        }
    }
}