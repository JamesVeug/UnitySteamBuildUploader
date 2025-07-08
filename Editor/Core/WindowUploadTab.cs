using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
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

        public override void Initialize(BuildUploaderWindow uploaderWindow)
        {
            base.Initialize(uploaderWindow);
            m_buildDescription = FormatDescription();
        }

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
                if (startButtonDisabled)
                {
                    string text = "Cannot continue: \nReason: " + reason;
                    EditorGUILayout.HelpBox(text, MessageType.Error);
                }
                
                using (new EditorGUI.DisabledScope(startButtonDisabled))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(GetBuildButtonText(), GUILayout.Height(100)))
                        {
                            if (EditorUtility.DisplayDialog("Start build and Upload",
                                    "Are you sure you want to start a new build then upload all enabled builds?" +
                                    "\n\nNOTE: You can not cancel this operation once started!",
                                    "Yes", "Cancel"))
                            {
                                BuildAndUpload();
                            }
                        }
                        
                        if (GUILayout.Button("Upload All", GUILayout.Height(100)))
                        {
                            if (EditorUtility.DisplayDialog("Upload All",
                                    "Are you sure you want to upload all enabled builds?" +
                                    "\n\nNOTE: You can not cancel this operation once started!",
                                    "Yes", "Cancel"))
                            {
                                DownloadAndUpload();
                            }
                        }
                    }
                }
            }
        }

        private string GetBuildButtonText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Build and Upload");
            sb.AppendLine(EditorUserBuildSettings.activeBuildTarget.ToString());


            List<string> flags = new List<string>();
            if (EditorUserBuildSettings.development)
                flags.Add("Development Build");

            if (EditorUserBuildSettings.allowDebugging)
                flags.Add("Allow Debugging");

            if (EditorUserBuildSettings.buildScriptsOnly)
                flags.Add("Build Scripts Only");

            if (EditorUserBuildSettings.connectProfiler)
                flags.Add("Connect Profiler");
            
            if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
                flags.Add("Deep Profiling Support");

            if (flags.Count > 0)
            {
                string flagsText = string.Join(", ", flags);
                sb.AppendLine($"({flagsText})");
            }
            

            return sb.ToString();
        }

        private void ShowEditDescriptionMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Clear"), false, () => m_buildDescription = "");
            menu.AddItem(new GUIContent("Reset"), false, () => m_buildDescription = FormatDescription());
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

        private async Task BuildAndUpload()
        {
            // Get where we should build to
            string buildPath = EditorPrefs.GetString("BuildUploader.BuildPath", "");
            if (!string.IsNullOrEmpty(buildPath))
            {
                buildPath = Path.GetDirectoryName(buildPath);
            }
            
            buildPath = EditorUtility.OpenFolderPanel("Select Build Folder", buildPath, "");
            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.Log("[BuildUploader] No build path selected. Aborting build and upload.");
                return;
            }
            
            EditorPrefs.SetString("BuildUploader.BuildPath", buildPath);

            // Validate the path so all configs reference the path
            int configsReferencingBuildPath = 0;
            int totalConfigs = 0;
            foreach (BuildConfig config in m_buildsToUpload)
            {
                if (!config.Enabled)
                    continue;

                totalConfigs++;
                foreach (BuildConfig.SourceData source in config.Sources)
                {
                    if (!source.Enabled)
                        continue;
                    
                    if (source.Source is ABrowsePathSource browsePathSource)
                    {
                        string sourcePath = browsePathSource.GetFullPath();
                        if (sourcePath.StartsWith(buildPath, StringComparison.OrdinalIgnoreCase))
                        {
                            configsReferencingBuildPath++;
                            break;
                        }
                    }
                }
            }
            
            if (totalConfigs != configsReferencingBuildPath)
            {
                if (!EditorUtility.DisplayDialog("Warning",
                        "1 or more Build Configs have no sources pointing to the build path. This may result in no files being uploaded. " +
                        "Are you sure you want to continue?", "Yes", "No"))
                {
                    Debug.Log("[BuildUploader] User cancelled the build and upload due to invalid build path references.");
                    return;
                }
            }

            // Build
            BuildReport report = await Build(buildPath);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.Log("[BuildUploader] Build failed! Skipping uploading step.");
                return;
            }

            // Upload
            DownloadAndUpload();
        }

        private async Task<BuildReport> Build(string buildPath)
        {
            // Get all enabled scenes in build settings
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

#if UNITY_STANDALONE_WIN
            string executableName = Application.productName + ".exe"; // For Windows, the executable is a .exe file
#elif UNITY_MAC
            string executableName = Application.productName + ".app"; // For macOS, the executable is a .app bundle
#else
            string executableName = Application.productName; // Default for other platforms
#endif

            BuildOptions buildOptions = BuildOptions.None;
            if (EditorUserBuildSettings.development)
                buildOptions |= BuildOptions.Development;

            if (EditorUserBuildSettings.allowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;

            if (EditorUserBuildSettings.buildScriptsOnly)
                buildOptions |= BuildOptions.BuildScriptsOnly;

            if (EditorUserBuildSettings.connectProfiler)
                buildOptions |= BuildOptions.ConnectWithProfiler;
            
            if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
                buildOptions |= BuildOptions.EnableDeepProfilingSupport;
            
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(buildPath, executableName),
                targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = buildOptions,
            };

            // Build the player
            return BuildPipeline.BuildPlayer(options);
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
            string taskReport = report.GetReport();
            if (Preferences.AutoSaveReportToCacheFolder)
            {
                string fileName = $"BuildReport_{guids}_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
                string reportPath = Path.Combine(Preferences.CacheFolderPath, fileName);
                try
                {
                    Debug.Log($"[BuildUploader] Writing build task report to {reportPath}");
                    await IOUtils.WriteAllTextAsync(reportPath, taskReport);
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
                if (Preferences.ShowConfirmationWindowAfterUpload)
                {
                    EditorUtility.DisplayDialog("Build Uploader", "All builds uploaded successfully!", "Yay!");
                }
            }
            else
            {
                Debug.LogError($"[BuildUploader] Build Task Failed! See logs for more info");
                Debug.Log($"[BuildUploader] {taskReport}");

                if (Preferences.ShowConfirmationWindowAfterUpload)
                {
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
            }

            if (Preferences.ShowReportAfterUpload)
            {
                BuildUploaderReportWindow.ShowWindow(report, taskReport);
            }
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

        private string FormatDescription()
        {
            string format = Preferences.DefaultDescriptionFormat;
            return format.Replace("{version}", Application.version);
        }
    }
}