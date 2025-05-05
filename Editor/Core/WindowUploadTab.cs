using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public class UploadTabData
        {
            [SerializeField] public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
        }

        public override string TabName => "Upload";
        
        private List<BuildConfig> m_buildsToUpload;

        private GUIStyle m_titleStyle;
        private Vector2 m_scrollPosition;
        private string m_buildDescription;
        private bool m_isDirty;
        private Vector2 m_descriptionScrollPosition;

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
                    BuildConfig newConfig = new BuildConfig(UploaderWindow);
                    newConfig.SetupDefaults();
                    m_buildsToUpload.Add(newConfig);
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
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Description:");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Edit", GUILayout.MaxWidth(50)))
                    {
                        ShowEditDescriptionMenu();
                    }
                }

                m_descriptionScrollPosition = GUILayout.BeginScrollView(m_descriptionScrollPosition, GUILayout.Height(100));
                m_buildDescription = GUILayout.TextArea(m_buildDescription, GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();

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

        private void ShowEditDescriptionMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Clear"), false, () => m_buildDescription = "");
            menu.AddItem(new GUIContent("Set/Text file"), false, () =>
            {
                // Choose file
                string path = EditorUtility.OpenFilePanel("Choose File", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                
                string text = File.ReadAllText(path);
                m_buildDescription = text;
            });
            menu.AddItem(new GUIContent("Append/Text file"), false, () =>
            {
                // Choose file
                string path = EditorUtility.OpenFilePanel("Choose File", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                
                string text = File.ReadAllText(path);
                m_buildDescription += "\n\n" + text;
            });
            menu.ShowAsContext();
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
            // Start task
            Debug.Log("[BuildUploader] Build Task started.... Grab a coffee... this could take a while.");
            BuildTask buildTask = new BuildTask(m_buildsToUpload, m_buildDescription);
            
            string guids = string.Join("_", m_buildsToUpload.Select(x => x.GUID));
            BuildTaskReport report = new BuildTaskReport(guids);
            Task asyncBuildTask = buildTask.Start(report);
            
            // Wait for task to complete
            while (!asyncBuildTask.IsCompleted)
            {
                // Wait for the task to complete
                await Task.Yield();
                UploaderWindow.Repaint();
            }

            // Write report to a txt file
            string fileName = $"BuildReport_{guids}_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
            string reportPath = Path.Combine(Preferences.CacheFolderPath, fileName);
            string taskReport = report.GetReport();
            if (Preferences.AutoSaveReportToCacheFolder)
            {
                try
                {
                    Debug.Log($"[BuildUploader] Writing build task report to {reportPath}");
                    await File.WriteAllTextAsync(reportPath, taskReport);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BuildUploader] Failed to write report to {reportPath}");
                    Debug.LogException(e);
                }
            }

            // Report back to the user
            if (report.Successful)
            {
                Debug.Log($"[BuildUploader] Build Task successful!");
                Debug.Log($"[BuildUploader] {taskReport}");
                EditorUtility.DisplayDialog("Build Uploader", "All builds uploaded successfully!", "Yay!");
            }
            else
            {
                Debug.LogError($"[BuildUploader] Build Task Failed! See logs for more info");
                Debug.Log($"[BuildUploader] {reportPath}");
                
                // Get the first 3 failed lines from the report
                StringBuilder sb = new StringBuilder();

                int logs = 0;
                foreach (var (stepType, log) in report.GetFailReasons())
                {
                    sb.AppendLine($"{stepType}: {log}");
                    logs++;
                    if (logs >= 3)
                    {
                        break;
                    }
                }

                sb.Append("\n\nSee logs for more info.");

                EditorUtility.DisplayDialog("Build Uploader", sb.ToString(), "Okay");
            }
            BuildUploaderReportWindow.ShowWindow(report, taskReport);
        }

        private bool CanStartBuild(out string reason)
        {
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
            if (validBuilds == 0)
            {
                reason = "No builds set up!";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_buildDescription))
            {
                reason = "No Description";
                return false;
            }

            reason = string.Empty;
            return true;
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
            Debug.Log("BuildUploader Saved build configs to: " + FilePath);
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
                        var jObject = config.Data[i];
                        buildConfig.Deserialize(jObject);
                        m_buildsToUpload.Add(buildConfig);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load build config: #" + (i+1));
                        Debug.LogException(e);
                        BuildConfig buildConfig = new BuildConfig(UploaderWindow);
                        m_buildsToUpload.Add(buildConfig);
                    }
                }
            }
        }
    }
}