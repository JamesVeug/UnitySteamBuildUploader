using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    /// <summary>
    /// Starts a new build using a BuildConfig.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(BuildConfigSource), "sources", "Chooses a BuildConfig to start a new build when uploading")]
    [UploadSource("BuildConfig", "Build Config")]
    public partial class BuildConfigSource : AUploadSource
    {
        public BuildConfig BuildConfig => m_BuildConfig;

        [Wiki("BuildConfig", "Which config to use when creating a build")]
        private BuildConfig m_BuildConfig = null;

        [Wiki("Override Target Platform", "If enabled, the target platform and architecture specified below will be used instead of the one in the BuildConfig")]
        private bool m_OverrideSwitchTargetPlatform;
        
        [WikiEnum("Target Platform", "The target platform to switch to before building. Only used if 'Override Switch Target Platform' is enabled.", false)]
        private BuildTarget m_Target;
        
        [Wiki("Target Architecture", "The target architecture to build for. Only used if 'Override Switch Target Platform' is enabled and the target platform supports multiple architectures.")]
        private BuildUtils.Architecture m_TargetArchitecture;
        
        [Wiki("Clean Build", "If enabled, the build folder will be deleted before building. This ensures a fresh build but may increase build time.")]
        private bool m_CleanBuild = false;
        
        // Also serialized but not exposed to WIKI
        private BuildTargetGroup m_TargetPlatform;
        private int m_TargetPlatformSubTarget;
        
        private string m_filePath = "";
        private bool m_appliedSettings = false;
        private BuildMetaData m_buildMetaData = null;
        private BuildConfig m_buildConfigToApply = null;
        
        // Lock to 1 build at a time regardless of how many tasks/configs are running
        internal static BuildConfig m_editorSettingsBeforeUpload = null;
        internal static SemaphoreSlim m_lock = new SemaphoreSlim(1);
        internal static int m_totalBuildsInProgress = 0;


        public override Task<bool> Prepare(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Create a new config since we make different changes at different times and this keeps it consistent
            m_buildConfigToApply = CreateSourceBuildConfig();

            return Task.FromResult(true);
        }

        private BuildConfig CreateSourceBuildConfig()
        {
            var config = new BuildConfig();
            config.Deserialize(m_BuildConfig.Serialize());

            if (m_OverrideSwitchTargetPlatform)
            {
                config.Target = m_Target;
                config.TargetArchitecture = m_TargetArchitecture;
                config.TargetPlatform = m_TargetPlatform;
                config.TargetPlatformSubTarget = m_TargetPlatformSubTarget;
                config.SwitchTargetPlatform = true;
            }
            else if (!config.SwitchTargetPlatform)
            {
                config.Target = BuildUtils.CurrentTargetPlatform();
                config.TargetArchitecture = BuildUtils.CurrentTargetArchitecture();
                config.TargetPlatform = BuildUtils.BuildTargetToPlatform();
                config.TargetPlatformSubTarget = BuildUtils.CurrentSubTarget();
                config.SwitchTargetPlatform = true;
            }
            
            return config;
        }

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Start build
            if (m_buildConfigToApply == null)
            {
                stepResult.AddError("No BuildConfig selected. Please select a BuildConfig to use.");
                token.Cancel();
                return false;
            }
            
            // Ensure only 1 build happens at a time to avoid applying settings over each other
            await m_lock.WaitAsync();
            if (token.IsCancellationRequested)
            {
                m_lock.Release();
                stepResult.AddLog("Build cancelled by user.");
                return false;
            }
            
            BuildReport report = null;
            try
            {
                m_buildMetaData = BuildUploaderProjectSettings.CreateFromProjectSettings(true);
                stepResult.AddLog("Build Number: " + m_buildMetaData.BuildNumber);

                m_filePath = GetBuiltDirectory(ctx);
                if (m_CleanBuild && Directory.Exists(m_filePath))
                {
                    // Clear the directory if it exists
                    try
                    {
                        stepResult.AddLog($"Clean build set and build already exists so deleting to make a fresh build: {m_filePath}");
                        Directory.Delete(m_filePath, true);
                    }
                    catch (Exception e)
                    {
                        stepResult.AddError($"Failed to clear build directory: {e.Message}");
                        stepResult.SetFailed("Failed to clear build directory. Please check the console for more details.");
                        return false;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    stepResult.AddLog("Build cancelled by user.");
                    return false;
                }

                if (!Directory.Exists(m_filePath))
                {
                    Directory.CreateDirectory(m_filePath);
                }
                
                // Cache current editor settings to restore later
                m_totalBuildsInProgress++;
                m_appliedSettings = true;
                if (m_editorSettingsBeforeUpload == null)
                {
                    m_editorSettingsBeforeUpload = new BuildConfig();
                    m_editorSettingsBeforeUpload.SetEditorSettings();
                    m_editorSettingsBeforeUpload.SwitchTargetPlatform = true;
                }

                if (!ApplyBuildConfig(m_buildConfigToApply, stepResult, ctx))
                {
                    return false;
                }

                // Get all enabled scenes in build settings
                BuildOptions buildOptions = m_buildConfigToApply.GetBuildOptions();
#if UNITY_2021_2_OR_NEWER
                buildOptions |= BuildOptions.DetailedBuildReport;
#endif

                string productName = m_buildConfigToApply.GetFormattedProductName(ctx);
                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes
                        .Where(scene => scene.enabled)
                        .Select(scene => scene.path)
                        .ToArray(),
                    locationPathName = Path.Combine(m_filePath, productName),
                    targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup,
                    target = EditorUserBuildSettings.activeBuildTarget,
                    options = buildOptions,
                };
            
                stepResult.AddLog("Starting build with the following options:");
                stepResult.AddLog($"Scenes: {string.Join(", ", options.scenes)}");
                stepResult.AddLog($"Location: {options.locationPathName}");
                stepResult.AddLog($"Target Group: {options.targetGroup}");
                stepResult.AddLog($"Target: {options.target}");
                stepResult.AddLog($"Build Options: {options.options}");
                

                // Build the player
                Stopwatch stopwatch = Stopwatch.StartNew();
                stepResult.AddLog("Starting build...");
                report = MakeBuild(options, stepResult, ctx);
                stopwatch.Stop();
                stepResult.AddLog($"Build completed in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                stepResult.AddException(e);
                stepResult.SetFailed("Build failed - " + e.Message);
                token.Cancel();
                return false;
            }
            finally
            {
                m_lock.Release();
                stepResult.SetPercentComplete(1f);
            }

            
            foreach (var step in report.steps)
            {
                stepResult.AddLog($"Step: {step.name}");
                stepResult.AddLog($"Duration: {step.duration}");
                foreach (BuildStepMessage message in step.messages)
                {
                    switch (message.type)
                    {
                        case LogType.Error:
                            stepResult.AddError(message.content);
                            break;
                        case LogType.Assert:
                            stepResult.AddLog(message.content);
                            break;
                        case LogType.Warning:
                            stepResult.AddWarning(message.content);
                            break;
                        case LogType.Log:
                            stepResult.AddLog(message.content);
                            break;
                        case LogType.Exception:
                            stepResult.AddError(message.content);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                stepResult.AddLog($"Build succeeded: {report.summary.totalSize} bytes");
                stepResult.AddLog($"Build path: {report.summary.outputPath}");
                return true;
            }

            string summarizeErrors = Utils.SummarizeErrors(report);
            stepResult.AddError(summarizeErrors);
            stepResult.SetFailed(summarizeErrors);
            token.Cancel();
            return false;
        }

        private string GetBuiltDirectory(StringFormatter.Context ctx)
        {
            BuildConfig config = BuildConfigContext;
            if (config == null)
            {
                return "";
            }
            string buildName = StringFormatter.FormatString(config.BuildName, ctx);
            string guid = config.GUID;
            string buildPath = string.Format("{0} ({1})", buildName, guid); // Development (1234)
            string targetName = BuildUtils.GetBuildPlatform(config.TargetPlatform, config.Target, config.TargetPlatformSubTarget).DisplayName;
            string platformPath = string.Format("{0} {1}", targetName, config.TargetArchitecture); // StandaloneWindows64 x64
            return Path.Combine(Preferences.CacheFolderPath, "BuildConfigBuilds", buildPath, platformPath);
        }

        private bool ApplyBuildConfig(BuildConfig config, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            stepResult?.AddLog($"Applying settings");
            if (!config.ApplySettings(config.SwitchTargetPlatform, ctx, stepResult))
            {
                stepResult?.SetFailed("Failed to apply build settings. Please check the console for more details.");
                return false;
            }
            
            stepResult?.AddLog($"Build settings applied");
            return true;
        }

        private BuildReport MakeBuild(BuildPlayerOptions options, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                LastBuildUtil.SetLastBuild(m_filePath, ctx.BuildName());
                if (BuildUploaderProjectSettings.Instance.IncludeBuildMetaDataInStreamingDataFolder)
                {
                    stepResult.AddLog("Saving build meta data to StreamingAssets folder");
                    BuildUploaderProjectSettings.SaveToStreamingAssets(m_buildMetaData, report.summary.outputPath);
                }
            }
            
            return report;
        }

        public override string SourceFilePath()
        {
            return m_filePath;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (!ValidConfig(out string reason))
            {
                errors.Add(reason);
            }
            else
            {
                // Stop the user from starting multiple tasks with the same config
                // This is because paths can be shared between configs and builds and can can break an active task
                foreach (UploadTask a in UploadTask.AllTasks)
                {
                    if (a.IsComplete) continue;
                    
                    foreach (UploadConfig b in a.UploadConfigs)
                    {
                        if(!b.Enabled) continue;
                        
                        foreach (UploadConfig.SourceData s in b.Sources)
                        {
                            if (!s.Enabled) continue;
                            if (s.Source is not BuildConfigSource otherSource) continue;
                            if (otherSource == this) continue;
                            
                            if (otherSource.BuildConfig == m_BuildConfig)
                            {
                                errors.Add($"BuildConfig '{m_BuildConfig.DisplayName}' is already used in another active Upload Task.");
                                goto exitLoop;
                            }
                        }
                    }
                }
                
                exitLoop: ;
            }
        }

        private bool ValidConfig(out string reason)
        {
            BuildConfig config = m_BuildConfig;
            if(config == null)
            {
                reason = "No BuildConfig selected.";
                return false;
            }

            // Check scene files exist
            if (config.SceneGUIDs.Count == 0)
            {
                reason = "No scenes selected in BuildConfig.";
                return false;
            }
            else
            {
                string[] sceneGUIDs = SceneUIUtils.GetSceneGUIDS();
                List<string> invalidScenes = new List<string>();
                foreach (string scene in config.SceneGUIDs)
                {
                    if (Array.IndexOf(sceneGUIDs, scene) == -1)
                    {
                        invalidScenes.Add($"'{scene}' not found.");
                    }
                }

                if (invalidScenes.Count > 0)
                {
                    reason = $"Invalid scene guids: {string.Join(", ", invalidScenes)}";
                    return false;
                }
            }

            bool switchPlatform = m_OverrideSwitchTargetPlatform || config.SwitchTargetPlatform;
            if (switchPlatform)
            {
                BuildTarget target = ResultingTarget();
                BuildTargetGroup targetGroup = ResultingTargetGroup();
                int subTarget = ResultingTargetPlatformSubTarget();

                BuildUtils.BuildPlatform buildPlatform = BuildUtils.GetBuildPlatform(targetGroup, target, subTarget);
                if (buildPlatform == null)
                {
                    reason = $"The selected target platform {target} ({targetGroup}) is not valid.";
                    return false;
                }
                
                if (!buildPlatform.installed)
                {
                    reason = $"The selected target platform {buildPlatform.DisplayName} is not installed.";
                    return false;
                }
            }
            
            reason = "";
            return true;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "BuildConfig", m_BuildConfig != null ? m_BuildConfig.GUID : "" },
                { "OverrideSwitchTargetPlatform", m_OverrideSwitchTargetPlatform },
                { "TargetPlatform", m_TargetPlatform.ToString() },
                { "TargetPlatformSubTarget", m_TargetPlatformSubTarget },
                { "Target", m_Target.ToString() },
                { "TargetArchitecture", (int)m_TargetArchitecture },
                { "CleanBuild", m_CleanBuild },
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("BuildConfig", out var buildConfigGuidObj) && buildConfigGuidObj is string buildConfigGuid)
            {
                m_BuildConfig = BuildConfigsUIUtils.GetBuildConfigs().FirstOrDefault(a=>a.GUID == buildConfigGuid);
                if (m_BuildConfig == null)
                {
                    Debug.LogWarning($"BuildConfig with GUID {buildConfigGuid} not found.");
                }
            }
            else
            {
                Debug.LogWarning("BuildConfig GUID not found in serialized data.");
            }
            
            if (data.TryGetValue("OverrideSwitchTargetPlatform", out var overrideSwitchTargetPlatformObj) && overrideSwitchTargetPlatformObj is bool overrideSwitchTargetPlatform)
            {
                m_OverrideSwitchTargetPlatform = overrideSwitchTargetPlatform;
            }
            else
            {
                m_OverrideSwitchTargetPlatform = false;
            }
            
            if (data.TryGetValue("TargetPlatform", out var targetPlatformObj) && targetPlatformObj is string targetPlatformStr && Enum.TryParse<BuildTargetGroup>(targetPlatformStr, out var targetPlatform))
            {
                m_TargetPlatform = targetPlatform;
            }
            else
            {
                m_TargetPlatform = BuildTargetGroup.Standalone;
            }
            
            if (data.TryGetValue("TargetPlatformSubTarget", out var targetPlatformSubTargetObj) && targetPlatformSubTargetObj is long targetPlatformSubTarget)
            {
                m_TargetPlatformSubTarget = (int)targetPlatformSubTarget;
            }
            else
            {
                m_TargetPlatformSubTarget = (int)StandaloneBuildSubtarget.Player;
            }
            
            if (data.TryGetValue("Target", out var targetObj) && targetObj is string targetStr && Enum.TryParse<BuildTarget>(targetStr, out var target))
            {
                m_Target = target;
            }
            else
            {
                m_Target = BuildTarget.StandaloneWindows64;
            }
            
            if (data.TryGetValue("TargetArchitecture", out var targetArchitectureObj) && targetArchitectureObj is int targetArchitecture)
            {
                m_TargetArchitecture = (BuildUtils.Architecture)targetArchitecture;
            }
            else
            {
                m_TargetArchitecture = BuildUtils.Architecture.x64;
            }
            
            if (data.TryGetValue("CleanBuild", out var cleanBuildObj) && cleanBuildObj is bool cleanBuild)
            {
                m_CleanBuild = cleanBuild;
            }
            else
            {
                m_CleanBuild = false;
            }
        }

        public override async Task CleanUp(int configIndex, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            await base.CleanUp(configIndex, stepResult, ctx);

            m_buildConfigToApply = null;
            m_buildMetaData = null;
            if (!m_appliedSettings)
            {
                stepResult.AddLog("No settings were applied so no need to restore previous settings.");
                return;
            }

            // Wait in case anything else is trying to apply settings or make a build
            await m_lock.WaitAsync();
            
            try
            {
                m_appliedSettings = false;
                m_totalBuildsInProgress--;
                if (m_totalBuildsInProgress > 0)
                {
                    // Another build or task is active and hasn't been cleaned up yet
                    stepResult.AddLog("Another build is still in progress so not restoring settings yet.");
                    return;
                }
                
                if (m_editorSettingsBeforeUpload == null)
                {
                    stepResult.AddLog("No previous editor settings to restore.");
                    return;
                }
                
                BuildConfig buildConfig = m_editorSettingsBeforeUpload;
                m_editorSettingsBeforeUpload = null;

                stepResult.AddLog($"Restoring previous editor settings... {buildConfig.TargetPlatform} ({buildConfig.TargetPlatformSubTarget}) {buildConfig.Target} {buildConfig.TargetArchitecture}");
                bool successful = buildConfig.ApplySettings(true, ctx);
                if (!successful)
                {
                    stepResult.AddError("Failed to restore previous build settings!");
                }
                else
                {
                    stepResult.AddLog("Previous editor settings restored.");
                }
            }
            finally
            {
                m_lock.Release();
            }
        }

        public BuildTarget ResultingTarget()
        {
            if (m_buildConfigToApply != null)
                return m_buildConfigToApply.Target;
            
            if (m_OverrideSwitchTargetPlatform)
                return m_Target;
            if (m_BuildConfig != null && m_BuildConfig.SwitchTargetPlatform)
                return m_BuildConfig.Target;
            return BuildUtils.CurrentTargetPlatform();
        }

        public BuildUtils.Architecture ResultingArchitecture()
        {
            if (m_buildConfigToApply != null)
                return m_buildConfigToApply.TargetArchitecture;
            
            if (m_OverrideSwitchTargetPlatform)
                return m_TargetArchitecture;
            if (m_BuildConfig != null && m_BuildConfig.SwitchTargetPlatform)
                return m_BuildConfig.TargetArchitecture;
            return BuildUtils.CurrentTargetArchitecture();
        }

        public int ResultingTargetPlatformSubTarget()
        {
            if (m_buildConfigToApply != null)
                return m_buildConfigToApply.TargetPlatformSubTarget;
            
            if (m_OverrideSwitchTargetPlatform)
                return m_TargetPlatformSubTarget;
            if (m_BuildConfig != null && m_BuildConfig.SwitchTargetPlatform)
                return m_BuildConfig.TargetPlatformSubTarget;
            return BuildUtils.CurrentSubTarget();
        }

        public BuildTargetGroup ResultingTargetGroup()
        {
            if (m_buildConfigToApply != null)
                return m_buildConfigToApply.TargetPlatform;
            
            if (m_OverrideSwitchTargetPlatform)
                return m_TargetPlatform;
            if (m_BuildConfig != null && m_BuildConfig.SwitchTargetPlatform)
                return m_BuildConfig.TargetPlatform;
            return BuildUtils.BuildTargetToPlatform();
        }

        public BuildUtils.BuildPlatform ResultingPlatform()
        {
            BuildTargetGroup group = ResultingTargetGroup();
            BuildTarget target = ResultingTarget();
            int subTarget = ResultingTargetPlatformSubTarget();
            return BuildUtils.GetBuildPlatform(group, target, subTarget);
        }
    }
}