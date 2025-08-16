using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Starts a new build using a BuildConfig.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(BuildConfigSource), "sources", "Chooses a BuildConfig to start a new build when uploading")]
    [UploadSource("BuildConfigSource", "Build Config")]
    public partial class BuildConfigSource : AUploadSource
    {
        [Wiki("BuildConfig", "Which config to use when creating a build")]
        protected BuildConfig m_BuildConfig = null;

        private string m_filePath = "";

        public override async Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult,
            StringFormatter.Context ctx)
        {
            // Start build
            if (m_BuildConfig == null)
            {
                stepResult.AddError("No BuildConfig selected. Please select a BuildConfig to use.");
                return false;
            }
            
            m_filePath = Path.Combine(Preferences.CacheFolderPath, "BuildConfigBuilds", m_BuildConfig.GUID);
            if (Directory.Exists(m_filePath))
            {
                // Clear the directory if it exists
                try
                {
                    Directory.Delete(m_filePath, true);
                }
                catch (Exception e)
                {
                    stepResult.AddError($"Failed to clear build directory: {e.Message}");
                    stepResult.SetFailed("Failed to clear build directory. Please check the console for more details.");
                    return false;
                }
            }
            
            Directory.CreateDirectory(m_filePath);
            
            // Get all enabled scenes in build settings
            BuildOptions buildOptions = BuildOptions.None;
            buildOptions |= BuildOptions.DetailedBuildReport;
            
            if (m_BuildConfig.IsDevelopmentBuild)
                buildOptions |= BuildOptions.Development;

            if (m_BuildConfig.AllowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;

            if (m_BuildConfig.BuildScriptsOnly)
                buildOptions |= BuildOptions.BuildScriptsOnly;

            if (m_BuildConfig.ConnectProfiler)
                buildOptions |= BuildOptions.ConnectWithProfiler;
            
            if (m_BuildConfig.EnableDeepProfilingSupport)
                buildOptions |= BuildOptions.EnableDeepProfilingSupport;

            string productName = StringFormatter.FormatString(m_BuildConfig.ProductName, ctx);
            string[] defines = m_BuildConfig.ExtraScriptingDefines.Select(a=>StringFormatter.FormatString(a, ctx)).ToArray();
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = m_BuildConfig.Scenes.Distinct().ToArray(),
                locationPathName = Path.Combine(m_filePath, productName),
                targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = buildOptions,
                extraScriptingDefines = defines,
            };
            
            stepResult.AddLog("Starting build with the following options:");
            stepResult.AddLog($"Scenes: {string.Join(", ", options.scenes)}");
            stepResult.AddLog($"Location: {options.locationPathName}");
            stepResult.AddLog($"Target Group: {options.targetGroup}");
            stepResult.AddLog($"Target: {options.target}");
            stepResult.AddLog($"Build Options: {options.options}");
 
            // Build the player
            BuildReport report = BuildPipeline.BuildPlayer(options);
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
            
            stepResult.AddError($"Build failed with result: {report.SummarizeErrors()}");
            stepResult.SetFailed(report.SummarizeErrors());
            return false;
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
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "BuildConfig", m_BuildConfig != null ? m_BuildConfig.GUID : "" }
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
        }
    }
}