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
            [SerializeField] public List<Dictionary<string, object>> PostUploads = new List<Dictionary<string, object>>();
        }

        public override string TabName => "Upload";
        
        private StringFormatter.Context m_context;
        private List<UploadConfig> m_buildsToUpload;
        private List<UploadConfig.PostUploadActionData> m_postUploadActions;

        private string m_buildPath;
        private bool m_showFormattedBuildPath = false;
        
        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        private Vector2 m_scrollPosition;
        private string m_buildDescription;
        private bool m_showFormattedDescription = false;
        private bool m_isDirty;
        private Vector2 m_descriptionScrollPosition;

        public override void Initialize(BuildUploaderWindow uploaderWindow)
        {
            base.Initialize(uploaderWindow);
            m_context = new StringFormatter.Context();
            m_context.TaskDescription = ()=> m_buildDescription;
            
            m_buildPath = EditorPrefs.GetString("BuildUploader.BuildPath", "");
            m_buildDescription = Preferences.DefaultDescriptionFormat;
        }

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            
            m_subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
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
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("New"))
                    {
                        UploadConfig newConfig = new UploadConfig(UploaderWindow);
                        newConfig.SetupDefaults();
                        m_buildsToUpload.Add(newConfig);
                        m_isDirty = true;
                    }
                    
                    string text = m_isDirty ? "*Save" : "Save";
                    if (GUILayout.Button(text, GUILayout.Width(100)))
                    {
                        Save();
                    }
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

                        UploadConfig uploadConfig = m_buildsToUpload[i];
                        bool e = EditorGUILayout.Toggle(uploadConfig.Enabled, GUILayout.Width(20));
                        if (e != uploadConfig.Enabled)
                        {
                            uploadConfig.Enabled = e;
                            m_isDirty = true;
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            uploadConfig.OnGUI(ref m_isDirty, m_context);
                        }

                        bool collapse = uploadConfig.Collapsed;
                        if (GUILayout.Button(collapse ? ">" : "\\/", GUILayout.Width(20)))
                        {
                            uploadConfig.Collapsed = !uploadConfig.Collapsed;
                        }
                    }
                }
                
                GUILayout.Space(10);
                
                // Post upload actions
                GUILayout.Label("Post Upload Actions", m_subTitleStyle);
                for (int i = 0; i < m_postUploadActions.Count; i++)
                {
                    UploadConfig.PostUploadActionData actionData = m_postUploadActions[i];
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Post Upload Action",
                                    "Are you sure you want to remove this post upload action?", "Yes"))
                            {
                                m_postUploadActions.RemoveAt(i--);
                                m_isDirty = true;
                                continue;
                            }
                        }

                        var status = (UploadConfig.PostUploadActionData.UploadCompleteStatus)EditorGUILayout.EnumFlagsField(actionData.WhenToExecute, GUILayout.Width(20));
                        if (status != actionData.WhenToExecute)
                        {
                            actionData.WhenToExecute = status;
                            m_isDirty = true;
                        }

                        bool disabled = actionData.WhenToExecute == UploadConfig.PostUploadActionData.UploadCompleteStatus.Never;
                        using (new EditorGUI.DisabledScope(disabled))
                        {
                            GUILayout.Label("Action Type: ", GUILayout.Width(100));
                            if (UIHelpers.ActionsPopup.DrawPopup(ref actionData.ActionType, GUILayout.Width(200)))
                            {
                                m_isDirty = true;
                                Utils.CreateInstance(actionData.ActionType?.Type, out actionData.UploadAction);
                            }
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            if (actionData.ActionType != null)
                            {
                                float maxWidth = UploaderWindow.position.width - 400;
                                if (actionData.Collapsed)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        actionData.UploadAction.OnGUICollapsed(ref m_isDirty, maxWidth, m_context);
                                    }
                                }
                                else
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        actionData.UploadAction.OnGUIExpanded(ref m_isDirty, m_context);
                                    }
                                }
                            }
                        }

                        bool collapse = actionData.Collapsed;
                        if (GUILayout.Button(collapse ? ">" : "\\/", GUILayout.Width(20)))
                        {
                            actionData.Collapsed = !actionData.Collapsed;
                        }
                    }
                }

                if (GUILayout.Button("Add"))
                {
                    // Show a popup to select an action
                    UploadConfig.PostUploadActionData actionData = new UploadConfig.PostUploadActionData();
                    actionData.SetupDefaults();
                    m_postUploadActions.Add(actionData);
                }
                

                if (m_isDirty && Preferences.AutoSaveBuildConfigsAfterChanges)
                {
                    Save();
                }

                EditorGUILayout.EndScrollView();


                GUILayout.FlexibleSpace();

                // Description
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIContent content = new GUIContent("F", EditorUtils.GetFormatStringTextFieldTooltip(m_context));
                    m_showFormattedDescription = GUILayout.Toggle(m_showFormattedDescription, content, "ToolbarButton", GUILayout.Width(20), GUILayout.Height(20));
                    
                    GUIContent label = new GUIContent("Build Description", "A description of the build that will be uploaded." +
                                                                           "\nDescription is included in some destinations such as Steamworks so keep it short." +
                                                                           "\nGood practice is to include the version number and a short summary of the changes since the last build." +
                                                                           "\nexample: v1.2.9 - Hotfix for missing player texture and balance changes.");
                    GUILayout.Label(label);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Edit", GUILayout.MaxWidth(50)))
                    {
                        ShowEditDescriptionMenu();
                    }
                }

                m_descriptionScrollPosition = GUILayout.BeginScrollView(m_descriptionScrollPosition, GUILayout.Height(100));
                if (m_showFormattedDescription)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        string formattedDescription = StringFormatter.FormatString(m_buildDescription, m_context);
                        GUILayout.TextArea(formattedDescription, GUILayout.ExpandHeight(true));
                    }
                }
                else
                {
                    m_buildDescription = GUILayout.TextArea(m_buildDescription, GUILayout.ExpandHeight(true));
                }

                GUILayout.EndScrollView();


                bool canUpload = CanStartUpload(out string reason);
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Build and upload
                    using (new EditorGUILayout.VerticalScope())
                    {
                        if (!canUpload)
                        {
                            string warning = "There are errors that may prevent uploading to start once a build is complete";
                            EditorGUILayout.HelpBox(warning, MessageType.Warning);
                        }
                        
                        DrawUploadButton();
                    }

                    // Upload all
                    using (new EditorGUILayout.VerticalScope())
                    {
                        if (!canUpload)
                        {
                            EditorGUILayout.HelpBox(reason, MessageType.Error);
                        }

                        using (new EditorGUI.DisabledScope(!canUpload))
                        {
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
        }

        private void DrawUploadButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Build Path", "The path where the new build will be saved.");
                GUILayout.Label(label, GUILayout.MaxWidth(70));

                if (EditorUtils.FormatStringTextField(ref m_buildPath, ref m_showFormattedBuildPath, m_context))
                {
                    EditorPrefs.SetString("BuildUploader.BuildPath", m_buildPath);
                }

                if (GUILayout.Button("...", GUILayout.MaxWidth(20)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Build Folder", StringFormatter.FormatString(m_buildPath, m_context), "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        m_buildPath = path;
                        EditorPrefs.SetString("BuildUploader.BuildPath", m_buildPath);
                    }
                }
                
                if (GUILayout.Button("Show", GUILayout.MaxWidth(100)))
                {
                    if (Directory.Exists(StringFormatter.FormatString(m_buildPath, m_context)))
                    {
                        EditorUtility.RevealInFinder(StringFormatter.FormatString(m_buildPath, m_context));
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Build path does not exist!", "OK");
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(GetBuildButtonText(), GUILayout.Height(80)))
                {
                    if (EditorUtility.DisplayDialog("Start build and Upload all",
                            "Are you sure you want to start a new build then upload all enabled builds?" +
                            "\nPath: " + StringFormatter.FormatString(m_buildPath, m_context) +
                            
                            "\n\nNOTE: You can not cancel this operation once started!",
                            "Yes", "Cancel"))
                    {
                        BuildAndUpload(StringFormatter.FormatString(m_buildPath, m_context));
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
            menu.AddItem(new GUIContent("Reset"), false, () => m_buildDescription = Preferences.DefaultDescriptionFormat);
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

        private async Task BuildAndUpload(string buildPath)
        {
            // Validate the path so all configs reference the path
            int configsReferencingBuildPath = 0;
            int totalConfigs = 0;
            foreach (UploadConfig config in m_buildsToUpload)
            {
                if (!config.Enabled)
                    continue;

                totalConfigs++;
                foreach (UploadConfig.SourceData source in config.Sources)
                {
                    if (!source.Enabled)
                        continue;
                    
                    if (source.Source is ABrowsePathSource browsePathSource)
                    {
                        string sourcePath = browsePathSource.GetFullPath(m_context);
                        if (sourcePath.StartsWith(buildPath, StringComparison.OrdinalIgnoreCase))
                        {
                            configsReferencingBuildPath++;
                            break;
                        }
                    }
                    else if (source.Source is LastUploadSource)
                    {
                        configsReferencingBuildPath++;
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
                Debug.LogError("[BuildUploader] Build failed! Skipping uploading step.");
                
                // report.SummarizeErrors() - Not valid in 2021
                StringBuilder summarizedErrors = new StringBuilder();
                summarizedErrors.AppendLine($"Build failed with result: {report.summary.result}");
                foreach (BuildStep step in report.steps)
                {
                    foreach (BuildStepMessage message in step.messages)
                    {
                        switch (message.type)
                        {
                            case LogType.Error:
                            case LogType.Exception:
                                summarizedErrors.AppendLine(message.content);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                Debug.LogError(summarizedErrors.ToString());
                
                
                foreach (BuildStep step in report.steps)
                {
                    foreach (BuildStepMessage message in step.messages)
                    {
                        switch (message.type)
                        {
                            case LogType.Error:
                                Debug.LogError($"[BuildUploader][{step.name}] {message.content}");
                                break;
                            case LogType.Assert:
                                Debug.LogError($"[BuildUploader][{step.name}] Assert: {message.content}");
                                break;
                            case LogType.Warning:
                                Debug.LogWarning($"[BuildUploader][{step.name}] Warning: {message.content}");
                                break;
                            case LogType.Log:
                                Debug.Log($"[BuildUploader][{step.name}] Log: {message.content}");
                                break;
                            case LogType.Exception:
                                Debug.LogException(new Exception($"[BuildUploader][{step.name}] Exception: {message.content}"));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                return;
            }

            // Make sure we CAN upload
            if (!CanStartUpload(out string reason))
            {
                Debug.LogError($"[BuildUploader] Build successful but can not start upload: {reason}");
                if (Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Always || 
                    Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Failed)
                {
                    // Show error dialog
                    EditorUtility.DisplayDialog("Build Uploader", reason, "Okay");
                }
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
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                LastBuildDirectoryUtil.LastBuildDirectory = Path.GetDirectoryName(report.summary.outputPath);
            }
            
            
            return report;
        }

        private async Task DownloadAndUpload()
        {
            // Start task
            Debug.Log("[BuildUploader] Build Task started.... Grab a coffee... this could take a while.");
            string description = StringFormatter.FormatString(m_buildDescription, m_context);
            UploadTask uploadTask = new UploadTask(m_buildsToUpload, description, m_postUploadActions);
            
            string guids = string.Join("_", m_buildsToUpload.Select(x => x.GUID));
            UploadTaskReport report = new UploadTaskReport(guids);
            Task asyncBuildTask = uploadTask.Start(report);
            
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
                string fileName = $"UploadReport_{guids}_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
                string reportPath = Path.Combine(Preferences.CacheFolderPath, fileName);
                try
                {
                    Debug.Log($"[BuildUploader] Writing upload task report to {reportPath}");
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
                Debug.Log($"[BuildUploader] Upload Task successful!");
                Debug.Log($"[BuildUploader] {taskReport}");
                if (Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Always || 
                    Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Successful)
                {
                    EditorUtility.DisplayDialog("Build Uploader", "All builds uploaded successfully!", "Yay!");
                }
                
                if (Preferences.ShowReportAfterUpload == Preferences.ShowIf.Always || 
                    Preferences.ShowReportAfterUpload == Preferences.ShowIf.Successful)
                {
                    UploadTaskReportWindow.ShowWindow(report, taskReport);
                }
            }
            else
            {
                Debug.LogError($"[BuildUploader] Build Task Failed! See logs for more info");
                Debug.Log($"[BuildUploader] {taskReport}");

                if (Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Always || 
                    Preferences.ShowConfirmationWindowAfterUpload == Preferences.ShowIf.Failed)
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
                
                if (Preferences.ShowReportAfterUpload == Preferences.ShowIf.Always ||
                    Preferences.ShowReportAfterUpload == Preferences.ShowIf.Failed)
                {
                    UploadTaskReportWindow.ShowWindow(report, taskReport);
                }
            }
        }

        private bool CanStartUpload(out string reason)
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

                if (!m_buildsToUpload[i].CanStartBuild(out string buildReason, m_context))
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
                m_buildsToUpload = new List<UploadConfig>();
            }

            UploadTabData data = new UploadTabData();
            for (int i = 0; i < m_buildsToUpload.Count; i++)
            {
                data.Data.Add(m_buildsToUpload[i].Serialize());
            }
            for (int i = 0; i < m_postUploadActions.Count; i++)
            {
                data.PostUploads.Add(m_postUploadActions[i].Serialize());
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
            // Debug.Log("BuildUploader Saved build configs to: " + FilePath);
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
                m_buildsToUpload = new List<UploadConfig>();
                m_postUploadActions = new List<UploadConfig.PostUploadActionData>();
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
                m_buildsToUpload = new List<UploadConfig>();
                m_postUploadActions = new List<UploadConfig.PostUploadActionData>();
                Save();
            }
            else
            {
                m_buildsToUpload = new List<UploadConfig>();
                for (int i = 0; i < config.Data.Count; i++)
                {
                    try
                    {
                        UploadConfig uploadConfig = new UploadConfig(UploaderWindow);
                        var jObject = config.Data[i];
                        uploadConfig.Deserialize(jObject);
                        m_buildsToUpload.Add(uploadConfig);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load build config: #" + (i+1));
                        Debug.LogException(e);
                        UploadConfig uploadConfig = new UploadConfig(UploaderWindow);
                        m_buildsToUpload.Add(uploadConfig);
                    }
                }
                
                m_postUploadActions = new List<UploadConfig.PostUploadActionData>();
                for (int i = 0; i < config.PostUploads.Count; i++)
                {
                    try
                    {
                        UploadConfig.PostUploadActionData actionData = new UploadConfig.PostUploadActionData();
                        actionData.Deserialize(config.PostUploads[i]);
                        m_postUploadActions.Add(actionData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load post upload action: #" + (i+1));
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}