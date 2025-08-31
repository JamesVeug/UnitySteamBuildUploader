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
        
        [Wiki("Clean Build", "If enabled, the build folder will be deleted before building. This ensures a fresh build but may increase build time.")]
        private bool m_CleanBuild = false;

        private string m_filePath = "";
        private bool m_appliedSettings = false;
        
        // Lock to 1 build at a time regardless of how many tasks/configs are running
        internal static BuildConfig m_editorSettingsBeforeUpload = null;
        internal static SemaphoreSlim m_lock = new SemaphoreSlim(1);
        internal static int m_totalBuildsInProgress = 0;

        private class Builds
        {
            public BuildConfig Config;
            public string TargetPath;
        }
        private static List<Builds> s_CompleteBuilds = new List<Builds>();

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx, CancellationTokenSource token)
        {
            // Start build
            if (m_BuildConfig == null)
            {
                stepResult.AddError("No BuildConfig selected. Please select a BuildConfig to use.");
                token.Cancel();
                return false;
            }
            
            // TODO: Consider multiple UploadTasks sequentially and if the user modified a build config mid upload
            
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
                // Check it's not already built to avoid building twice
                // Builds completeBuild = s_CompleteBuilds.FirstOrDefault(a => a.Config == m_BuildConfig);
                // if (!m_CleanBuild && completeBuild != null)
                // {
                //     m_filePath = completeBuild.TargetPath;
                //     stepResult.AddLog($"Build already completed for {m_BuildConfig.DisplayName}, reusing existing build at path {m_filePath}");
                //     return true;
                // }
                
                m_filePath = Path.Combine(Preferences.CacheFolderPath, "BuildConfigBuilds", m_BuildConfig.GUID);
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
                    m_editorSettingsBeforeUpload.SetupDefaults();
                    m_editorSettingsBeforeUpload.SwitchTargetPlatform = true;
                }
                
                stepResult.AddLog($"Applying settings");
                if (!m_BuildConfig.ApplySettings(ctx, stepResult))
                {
                    stepResult.SetFailed("Failed to apply build settings. Please check the console for more details.");
                    return false;
                }
                stepResult.AddLog($"Build settings applied");
            
                // Get all enabled scenes in build settings
                BuildOptions buildOptions = m_BuildConfig.GetBuildOptions();
#if UNITY_2021_2_OR_NEWER
                buildOptions |= BuildOptions.DetailedBuildReport;
#endif

                string productName = m_BuildConfig.GetFormattedProductName(ctx);
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
                report = MakeBuild(options, stepResult);
                stopwatch.Stop();
                stepResult.AddLog($"Build completed in {stopwatch.ElapsedMilliseconds} ms");

                if (report.summary.result == BuildResult.Succeeded)
                {
                    // s_CompleteBuilds.Add(new Builds
                    // {
                    //     Config = m_BuildConfig,
                    //     TargetPath = m_filePath
                    // });

                    LastBuildUtil.SetLastBuild(m_filePath, ctx.BuildName());
                }
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

        private static BuildReport MakeBuild(BuildPlayerOptions options, UploadTaskReport.StepResult stepResult)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                if (BuildUploaderProjectSettings.Instance.IncludeBuildMetaDataInStreamingDataFolder)
                {
                    stepResult.AddLog("Saving build meta data to StreamingAssets folder");
                    BuildMetaData buildMetaData = BuildUploaderProjectSettings.CreateFromProjectSettings(true);
                    BuildUploaderProjectSettings.SaveToStreamingAssets(buildMetaData, report.summary.outputPath);
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
            
            if (m_BuildConfig == null)
            {
                errors.Add("No BuildConfig selected. Please select a BuildConfig to use.");
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

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "BuildConfig", m_BuildConfig != null ? m_BuildConfig.GUID : "" },
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
            
            if (data.TryGetValue("CleanBuild", out var cleanBuildObj) && cleanBuildObj is bool cleanBuild)
            {
                m_CleanBuild = cleanBuild;
            }
            else
            {
                m_CleanBuild = false;
            }
        }

        public override async Task CleanUp(int i, UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            await base.CleanUp(i, result, ctx);

            if (!m_appliedSettings)
            {
                result.AddLog("No settings were applied so no need to restore previous settings.");
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
                    result.AddLog("Another build is still in progress so not restoring settings yet.");
                    return;
                }
                
                if (m_editorSettingsBeforeUpload == null)
                {
                    result.AddLog("No previous editor settings to restore.");
                    return;
                }
                
                BuildConfig buildConfig = m_editorSettingsBeforeUpload;
                m_editorSettingsBeforeUpload = null;

                result.AddLog("Restoring previous editor settings...");
                bool successful = buildConfig.ApplySettings(ctx);
                if (!successful)
                {
                    result.AddError("Failed to restore previous build settings!");
                }
                else
                {
                    result.AddLog("Previous editor settings restored.");
                }
            }
            finally
            {
                m_lock.Release();
            }
        }
    }
}