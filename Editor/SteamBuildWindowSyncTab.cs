using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindowSyncTab : SteamBuildWindowTab
    {
        private static readonly string FilePath = Application.persistentDataPath + "/SteamBuilder/WindowSyncTab.json";

        [Serializable]
        public class SyncTabData
        {
            [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        }

        private List<SteamBuild> m_buildsToSync;

        private GUIStyle m_titleStyle;
        private Vector2 m_scrollPosition;
        private string m_buildDescription;

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            if (m_buildsToSync == null)
                Load();
        }

        public override void OnGUI()
        {
            Setup();

            using (new GUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button("Save"))
                {
                    Save();
                }

                GUILayout.Label("Builds to sync", m_titleStyle);

                if (GUILayout.Button("New"))
                {
                    SteamBuild buildSetup = new SteamBuild(window);
                    buildSetup.Collapsed = true;
                    m_buildsToSync.Add(buildSetup);
                }

                // Builds to sync
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (int i = 0; i < m_buildsToSync.Count; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("X"))
                        {
                            if (EditorUtility.DisplayDialog("Remove Build",
                                    "Are you sure you want to remove this build to sync?", "Yes"))
                            {
                                m_buildsToSync.RemoveAt(i--);
                                continue;
                            }
                        }

                        SteamBuild steamBuild = m_buildsToSync[i];
                        steamBuild.Enabled = EditorGUILayout.Toggle(steamBuild.Enabled, GUILayout.Width(20));

                        using (new GUILayout.VerticalScope())
                        {
                            steamBuild.OnGUI();
                        }

                        bool collapse = steamBuild.Collapsed;
                        if (GUILayout.Button(collapse ? "+" : "-"))
                        {
                            steamBuild.Collapsed = !steamBuild.Collapsed;
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
                                "Are you sure you want to sync all builds?",
                                "Yes", "Cancel"))
                        {
                            EditorCoroutineUtility.StartCoroutine(SyncAndUpload(), window);
                        }
                    }
                }
            }
        }

        private IEnumerator SyncAndUpload()
        {
            // Start uploading
            SteamWindowBuildProgressWindow buildProgressWindow =
                new SteamWindowBuildProgressWindow(m_buildsToSync, m_buildDescription);
            IEnumerator startProgress = buildProgressWindow.StartProgress();
            yield return startProgress;
        }

        private bool CanStartBuild()
        {
            if (string.IsNullOrEmpty(m_buildDescription))
            {
                return false;
            }

            if (m_buildsToSync == null)
            {
                return false;
            }

            int validBuilds = 0;
            for (int i = 0; i < m_buildsToSync.Count; i++)
            {
                if (!m_buildsToSync[i].Enabled)
                    continue;

                if (!m_buildsToSync[i].CanStartBuild())
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
            if (m_buildsToSync == null)
            {
                m_buildsToSync = new List<SteamBuild>();
            }

            SyncTabData data = new SyncTabData();
            for (int i = 0; i < m_buildsToSync.Count; i++)
            {
                data.Data.Add(m_buildsToSync[i].Serialize());
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
                SyncTabData config = JsonUtility.FromJson<SyncTabData>(json);
                if (config == null)
                {
                    Debug.Log("Config is null. Creating new config");
                    m_buildsToSync = new List<SteamBuild>();
                    Save();
                }
                else
                {
                    m_buildsToSync = new List<SteamBuild>();
                    for (int i = 0; i < config.Data.Count; i++)
                    {
                        SteamBuild steamBuild = new SteamBuild(window);
                        steamBuild.Collapsed = true;
                        var jObject = config.Data[i];
                        steamBuild.Deserialize(jObject);
                        m_buildsToSync.Add(steamBuild);
                    }
                }
            }
            else
            {
                Debug.Log("SteamBuildData does not exist. Creating new file");
                m_buildsToSync = new List<SteamBuild>();
                Save();
            }
        }
    }
}