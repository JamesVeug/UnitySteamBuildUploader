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
        internal static readonly string UploadProfilePath =  Application.dataPath + "/../BuildUploader/UploadProfiles";
        internal static readonly string UploadReportSaveDirectory = Path.Combine(Preferences.CacheFolderPath, "Upload Task Reports");

        public override string TabName => "Upload";
        
        private StringFormatter.Context m_context;
        private List<UploadProfileMeta> m_unloadedUploadProfiles;
        private UploadProfile m_currentUploadProfile;
        private UploadProfileMeta m_currentUploadProfileData;

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
            m_context.TaskProfileName = () => m_currentUploadProfile?.ProfileName ?? "No Profile Selected";
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

            if (m_unloadedUploadProfiles == null)
                Load();
        }

        public override void OnGUI()
        {
            Setup();

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label("Upload Configs", m_titleStyle);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("New Upload Config"))
                    {
                        UploadConfig newConfig = new UploadConfig();
                        newConfig.SetupDefaults();
                        m_currentUploadProfile.UploadConfigs.Add(newConfig);
                        m_isDirty = true;
                    }
                    
                    // Dropdown to select upload profile
                    List<string> profileNames = new List<string>();
                    profileNames.Add("-- Select Upload Profile --");
                    
                    profileNames.AddRange(m_unloadedUploadProfiles.Select(p => p.ProfileName));
                    int selectedIndex = m_unloadedUploadProfiles.FindIndex(a=>a.GUID == m_currentUploadProfile.GUID);
                    if (selectedIndex != -1)
                    {
                        selectedIndex++;
                    }
                    
                    var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, profileNames.ToArray(), GUILayout.Width(150));
                    if (newSelectedIndex < 0)
                    {
                        newSelectedIndex = selectedIndex;
                    }
                    
                    if (newSelectedIndex != selectedIndex && newSelectedIndex > 0)
                    {
                        if (m_isDirty)
                        {
                            if (EditorUtility.DisplayDialog("Unsaved Changes",
                                    "You have unsaved changes. Do you want to save them before switching profiles?",
                                    "Yes", "No"))
                            {
                                Save();
                            }
                        }
                        LoadMetaDataFromPath(m_unloadedUploadProfiles[newSelectedIndex - 1]);
                    }

                    if (!Preferences.AutoSaveUploadConfigsAfterChanges)
                    {
                        string text = m_isDirty ? "*Save" : "Save";
                        if (GUILayout.Button(text, GUILayout.Width(100)))
                        {
                            Save();
                        }
                    }

                    string dropdownText = m_isDirty ? "*" : "";
                    if (GUILayout.Button(Utils.SettingsIcon, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent(dropdownText + "Save"), false, Save);
                        
                        menu.AddItem(new GUIContent("Rename"), false, () =>
                        {
                            TextInputPopup.ShowWindow(newName =>
                            {
                                if (!string.IsNullOrEmpty(newName))
                                {
                                    m_currentUploadProfile.ProfileName = newName;
                                    m_currentUploadProfileData.ProfileName = newName;
                                    m_isDirty = true;
                                    // Save or update as needed
                                }
                            });
                        });

                        if (m_unloadedUploadProfiles.Count > 1)
                        {
                            menu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Delete Upload Profile",
                                        "Are you sure you want to delete this upload profile?", "Yes", "No"))
                                {
                                    int index = m_unloadedUploadProfiles.FindIndex(a =>
                                        a.GUID == m_currentUploadProfile.GUID);
                                    UploadProfileMeta meta = m_unloadedUploadProfiles[index];
                                    if (!string.IsNullOrEmpty(meta.FilePath))
                                    {
                                        File.Delete(meta.FilePath);
                                    }

                                    m_unloadedUploadProfiles.RemoveAt(index);
                                    int newIndex = Mathf.Clamp(index, 0, m_unloadedUploadProfiles.Count - 1);
                                    LoadMetaDataFromPath(m_unloadedUploadProfiles[newIndex]);
                                    m_isDirty = true;
                                }
                            });
                        }
                        
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("-- Create New Upload Profile --"), false, () =>
                        {
                            TextInputPopup.ShowWindow((profileName) =>
                            {
                                if (!string.IsNullOrEmpty(profileName))
                                {
                                    CreateDefaultUploadConfig(profileName);
                                    Save();
                                }
                            });
                        });

                        menu.ShowAsContext();
                    }
                }

                // Builds to upload
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (int i = 0; i < m_currentUploadProfile.UploadConfigs.Count; i++)
                {
                    UploadConfig uploadConfig = m_currentUploadProfile.UploadConfigs[i];
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (CustomFoldoutButton.OnGUI(uploadConfig.Collapsed))
                        {
                            uploadConfig.Collapsed = !uploadConfig.Collapsed;
                        }

                        bool e = EditorGUILayout.Toggle(uploadConfig.Enabled, GUILayout.Width(20));
                        if (e != uploadConfig.Enabled)
                        {
                            uploadConfig.Enabled = e;
                            m_isDirty = true;
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            uploadConfig.OnGUI(UploaderWindow.position.width, ref m_isDirty, m_context);
                        }
                        
                        if (GUILayout.Button(Utils.SettingsIcon, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Move Up"), false, () =>
                            {
                                int indexOf = m_currentUploadProfile.UploadConfigs.IndexOf(uploadConfig);
                                if (indexOf > 0)
                                {
                                    m_currentUploadProfile.UploadConfigs.RemoveAt(indexOf);
                                    m_currentUploadProfile.UploadConfigs.Insert(indexOf - 1, uploadConfig);
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddItem(new GUIContent("Move Down"), false, () =>
                            {
                                int indexOf = m_currentUploadProfile.UploadConfigs.IndexOf(uploadConfig);
                                if (indexOf < m_currentUploadProfile.UploadConfigs.Count - 1)
                                {
                                    m_currentUploadProfile.UploadConfigs.RemoveAt(indexOf);
                                    m_currentUploadProfile.UploadConfigs.Insert(indexOf + 1, uploadConfig);
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Remove Upload Config",
                                        "Are you sure you want to remove this Upload Config?", "Delete", "Cancel"))
                                {
                                    m_currentUploadProfile.UploadConfigs.Remove(uploadConfig);
                                    m_isDirty = true;
                                }
                            });
                            menu.ShowAsContext();
                        }
                    }
                }
                
                GUILayout.FlexibleSpace();
                
                // Post upload actions
                GUILayout.Label("Post Upload Actions", m_subTitleStyle);
                for (int i = 0; i < m_currentUploadProfile.PostUploadActions.Count; i++)
                {
                    UploadConfig.PostUploadActionData actionData = m_currentUploadProfile.PostUploadActions[i];
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (EditorGUILayout.DropdownButton(new GUIContent(""), FocusType.Passive, GUILayout.Width(20)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("MoveUp"), false, () =>
                            {
                                int indexOf = m_currentUploadProfile.PostUploadActions.IndexOf(actionData);
                                if (indexOf > 0)
                                {
                                    m_currentUploadProfile.PostUploadActions.RemoveAt(indexOf);
                                    m_currentUploadProfile.PostUploadActions.Insert(indexOf - 1, actionData);
                                    m_isDirty = true;
                                }
                            });
                            menu.AddItem(new GUIContent("MoveDown"), false, () =>
                            {
                                int indexOf = m_currentUploadProfile.PostUploadActions.IndexOf(actionData);
                                if (indexOf < m_currentUploadProfile.PostUploadActions.Count - 1)
                                {
                                    m_currentUploadProfile.PostUploadActions.RemoveAt(indexOf);
                                    m_currentUploadProfile.PostUploadActions.Insert(indexOf + 1, actionData);
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Remove Post Upload Action",
                                        "Are you sure you want to remove this post upload action?", "Delete", "Cancel"))
                                {
                                    m_currentUploadProfile.PostUploadActions.Remove(actionData);
                                    m_isDirty = true;
                                }
                            });
                            menu.ShowAsContext();
                        }

                        var status = (UploadConfig.PostUploadActionData.UploadCompleteStatus)EditorGUILayout.EnumPopup(actionData.WhenToExecute, GUILayout.Width(100));
                        if (status != actionData.WhenToExecute)
                        {
                            actionData.WhenToExecute = status;
                            m_isDirty = true;
                        }

                        bool disabled = actionData.WhenToExecute == UploadConfig.PostUploadActionData.UploadCompleteStatus.Never;
                        using (new EditorGUI.DisabledScope(disabled))
                        {
                            // GUILayout.Label("Action Type: ", GUILayout.Width(100));
                            if (UIHelpers.ActionsPopup.DrawPopup(ref actionData.ActionType, GUILayout.Width(200)))
                            {
                                m_isDirty = true;
                                Utils.CreateInstance(actionData.ActionType?.Type, out actionData.UploadAction);
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
                    m_currentUploadProfile.PostUploadActions.Add(actionData);
                }
                

                if (m_isDirty && Preferences.AutoSaveUploadConfigsAfterChanges)
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
                        UploaderWindow.SetTab<WindowTasksTab>();
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
            foreach (UploadConfig config in m_currentUploadProfile.UploadConfigs)
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
            if (m_isDirty)
            {
                if (EditorUtility.DisplayDialog("Un-saved changes",
                        "Are you sure you want to start uploading?\nThe unsaved changes will not be applied to this upload.", "Save Now", "Cancel"))
                {
                    Save();
                }
                else
                {
                    Debug.Log("[BuildUploader] Upload Task cancelled at user request. Unsaved changes.");
                    return;
                }
            }
            
            // Setup Task
            UploadProfile uploadProfile = UploadProfile.FromPath(m_currentUploadProfileData.FilePath);
            string description = StringFormatter.FormatString(m_buildDescription, m_context);
            UploadTask uploadTask = new UploadTask(uploadProfile, description);
            
            string guids = string.Join("_", m_currentUploadProfile.UploadConfigs.Select(x => x.GUID));
            
            // Start task
            Debug.Log("[BuildUploader] Upload Task started.... Grab a coffee... this could take a while.");
            WindowTasksTab taskTab = UploaderWindow.SetTab<WindowTasksTab>();
            if (taskTab != null)
            {
                taskTab.ShowTask(uploadTask);
            }
            
            await Task.Yield(); // Yield to allow the UI to update before starting the task
            await Task.Yield(); // Yield to allow the UI to update before starting the task
            await Task.Yield(); // Yield to allow the UI to update before starting the task
            Task asyncBuildTask = uploadTask.StartAsync(true);
            
            // Wait for task to complete
            while (!asyncBuildTask.IsCompleted)
            {
                // Wait for the task to complete
                await Task.Yield();
                UploaderWindow.Repaint();
            }
            
            // TODO: Move this logic out of this window so CIs can show them too

            // Write report to a txt file
            UploadTaskReport report = uploadTask.Report;
            string taskReport = report.GetReport();
            if (Preferences.AutoSaveReportToCacheFolder)
            {
                string fileName = $"UploadReport_{guids}_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
                string reportPath = Path.Combine(UploadReportSaveDirectory, fileName);
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
            if (m_currentUploadProfile.UploadConfigs == null)
            {
                reason = "No builds to upload!";
                return false;
            }

            int validBuilds = 0;
            for (int i = 0; i < m_currentUploadProfile.UploadConfigs.Count; i++)
            {
                if (!m_currentUploadProfile.UploadConfigs[i].Enabled)
                    continue;

                if (!m_currentUploadProfile.UploadConfigs[i].CanStartBuild(out string buildReason, m_context))
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
            if (m_currentUploadProfile == null)
            {
                return;
            }

            UploadProfileSavedData data = UploadProfileSavedData.FromUploadProfile(m_currentUploadProfile);
            
            string filePath = m_currentUploadProfileData.FilePath;
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JSON.SerializeObject(data);
            if (!File.Exists(filePath))
            {
                var stream = File.Create(filePath);
                stream.Close();
            }

            File.WriteAllText(filePath, json);
        }

        public void Load()
        {
            m_unloadedUploadProfiles = new List<UploadProfileMeta>();
            m_currentUploadProfile = null;

            if (!Directory.Exists(UploadReportSaveDirectory))
            {
                MoveOldUploadReportsToNewFolder();
            }
            
            if (!Directory.Exists(UploadProfilePath))
            {
                Directory.CreateDirectory(UploadProfilePath);
                
                // In v3.0.0 we moved the using multiple upload profiles and put them in the Projects/BuildUploader folder
                if (File.Exists(Application.persistentDataPath + "/BuildUploader/WindowUploadTab.json"))
                {
                    LoadOldUploadTabDataFromPath(Application.persistentDataPath + "/BuildUploader/WindowUploadTab.json");
                    
                    // TODO: Delete the old file
                    Save();
                    return;
                }
                else if (File.Exists(Application.persistentDataPath + "/SteamBuilder/WindowUploadTab.json"))
                {
                    // In v2.0.0 we renamed from UnitySteamBuildBuilder to BuildUploader
                    Debug.Log("SteamBuildData exists from a previous version. Migrating it over");
                    LoadOldUploadTabDataFromPath(Application.persistentDataPath + "/SteamBuilder/WindowUploadTab.json");
                    
                    // TODO: Delete the old file
                    Save();
                    return;
                }
            }
            else
            {
                string[] files = Directory.GetFiles(UploadProfilePath, "*.json");
                if (files.Length > 0)
                {
                    for (int j = 0; j < files.Length; j++)
                    {
                        string file = files[j];
                        string json = File.ReadAllText(file);
                        UploadProfileSavedData savedData = JSON.DeserializeObject<UploadProfileSavedData>(json);
                        if (savedData == null)
                        {
                            Debug.LogWarning($"Failed to deserialize UploadProfileSavedData from file: {file}. Skipping this file.");
                            continue;
                        }

                        if (string.IsNullOrEmpty(savedData.GUID))
                        {
                            savedData.GUID = Guid.NewGuid().ToString().Substring(0, 6);
                        }

                        UploadProfileMeta metaData = new UploadProfileMeta();
                        metaData.GUID = savedData.GUID;
                        metaData.ProfileName = savedData.ProfileName;
                        metaData.FilePath = file;
                        m_unloadedUploadProfiles.Add(metaData);
                    }

                    if (m_unloadedUploadProfiles.Count > 0)
                    {
                        m_unloadedUploadProfiles.Sort((a,b)=>String.Compare(a.ProfileName, b.ProfileName, StringComparison.Ordinal));
                        
                        string previouslySelectedGUID = ProjectEditorPrefs.GetString("BuildUploader.LastSelectedUploadProfileGUID", string.Empty);
                        if (string.IsNullOrEmpty(previouslySelectedGUID))
                        {
                            LoadMetaDataFromPath(m_unloadedUploadProfiles[0]);
                        }
                        else if (m_unloadedUploadProfiles.Any(x => x.GUID == previouslySelectedGUID))
                        {
                            // Load the previously selected profile
                            UploadProfileMeta metaData = m_unloadedUploadProfiles.First(x => x.GUID == previouslySelectedGUID);
                            LoadMetaDataFromPath(metaData);
                        }
                        else
                        {
                            // Load the first profile
                            LoadMetaDataFromPath(m_unloadedUploadProfiles[0]);
                        }
                        return;
                    }
                }
            }
            
            Debug.Log("No Upload profiles exist. Creating new file");
            CreateDefaultUploadConfig();
            Save();
        }

        private void MoveOldUploadReportsToNewFolder()
        {
            if (!Directory.Exists(UploadReportSaveDirectory))
            {
                Directory.CreateDirectory(UploadReportSaveDirectory);
            }
            
            string oldReportPath = Path.Combine(Preferences.CacheFolderPath);
            string[] reportFiles = Directory.GetFiles(oldReportPath, "UploadReport_*.txt", SearchOption.TopDirectoryOnly);
            foreach (string file in reportFiles)
            {
                // Move to new directory
                string fileName = Path.GetFileName(file);
                string newFilePath = Path.Combine(UploadReportSaveDirectory, fileName);
                if (!File.Exists(newFilePath))
                {
                    try{
                        File.Move(file, newFilePath);
                        Debug.Log($"Moved old upload report from {file} to {newFilePath}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to move old upload report from {file} to {newFilePath}");
                        Debug.LogException(e);
                    }
                }
                else
                {
                    Debug.LogWarning($"Upload report file already exists in new directory: {newFilePath}. Skipping move.");
                }
            }
        }

        private void CreateDefaultUploadConfig(string profileName = "Default")
        {
            UploadProfile defaultProfile = new UploadProfile();
            defaultProfile.GUID = Guid.NewGuid().ToString().Substring(0, 6);
            defaultProfile.ProfileName = profileName;
            m_currentUploadProfile = defaultProfile;
            
            UploadProfileMeta defaultMetaData = new UploadProfileMeta();
            defaultMetaData.GUID = defaultProfile.GUID;
            defaultMetaData.ProfileName = defaultProfile.ProfileName;
            defaultMetaData.FilePath = Path.Combine(UploadProfilePath, $"{defaultMetaData.GUID}.json");
            m_unloadedUploadProfiles.Add(defaultMetaData);
            m_currentUploadProfileData = defaultMetaData;
        }

        private void LoadMetaDataFromPath(UploadProfileMeta metaData)
        {
            string json = File.ReadAllText(metaData.FilePath);
            UploadProfileSavedData savedData = JSON.DeserializeObject<UploadProfileSavedData>(json);
            if (string.IsNullOrEmpty(savedData.GUID))
            {
                savedData.GUID = metaData.GUID;
            }
            
            UploadProfile loadedProfile = savedData.ToUploadProfile();
            m_currentUploadProfile = loadedProfile;
            m_currentUploadProfileData = metaData;
            
            ProjectEditorPrefs.SetString("BuildUploader.LastSelectedUploadProfileGUID", metaData.GUID);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void LoadOldUploadTabDataFromPath(string filePath)
        {
            string json = File.ReadAllText(filePath);
            UploadTabData config = JSON.DeserializeObject<UploadTabData>(json);
            if (config == null)
            {
                Debug.Log("Could not deserialize old JSON. Creating new default Profile");
                CreateDefaultUploadConfig();
                Save();
            }
            else
            {
                CreateDefaultUploadConfig();
                UploadProfile defaultProfile = m_currentUploadProfile;
                for (int i = 0; i < config.Data.Count; i++)
                {
                    try
                    {
                        UploadConfig uploadConfig = new UploadConfig();
                        var jObject = config.Data[i];
                        uploadConfig.Deserialize(jObject);
                        defaultProfile.UploadConfigs.Add(uploadConfig);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load build config: #" + (i+1));
                        Debug.LogException(e);
                        UploadConfig uploadConfig = new UploadConfig();
                        defaultProfile.UploadConfigs.Add(uploadConfig);
                    }
                }
                
                for (int i = 0; i < config.PostUploads.Count; i++)
                {
                    try
                    {
                        UploadConfig.PostUploadActionData actionData = new UploadConfig.PostUploadActionData();
                        actionData.Deserialize(config.PostUploads[i]);
                        defaultProfile.PostUploadActions.Add(actionData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load post upload action: #" + (i+1));
                        Debug.LogException(e);
                    }
                }
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    public class UploadProfileMeta
    {
        public string GUID;
        public string ProfileName;
        public string FilePath;

        public UploadProfileMeta()
        {
            
        }
    }
}