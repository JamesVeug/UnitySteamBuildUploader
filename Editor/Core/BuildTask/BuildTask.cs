using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class BuildTask
    {
        public List<BuildConfig> BuildConfigs => buildConfigs;
        public string BuildDescription => buildDescription;
        public string[] CachedLocations => cachedLocations;
        
        private List<BuildConfig> buildConfigs;
        private string[] cachedLocations;
        private int progressId;
        private string buildDescription;

        public BuildTask(List<BuildConfig> buildConfigs, string buildDescription)
        {
            this.buildDescription = buildDescription;
            this.buildConfigs = buildConfigs;
        }
        
        public BuildTask()
        {
            buildDescription = "";
            buildConfigs = new List<BuildConfig>();
        }

        ~BuildTask()
        {
            if (ProgressUtils.Exists(progressId))
            {
                ProgressUtils.Remove(progressId);
            }
        }

        public async Task Start(BuildTaskReport report, Action tick = null, Action<bool> onComplete = null)
        {
            progressId = ProgressUtils.Start("Build Uploader Window", "Upload Builds");
            cachedLocations = new string[buildConfigs.Count];

            ABuildTask_Step[] steps = new ABuildTask_Step[]
            {
                new BuildTaskStep_GetSources(), // Download content from services or get local folder
                new BuildTaskStep_CacheSources(), // Cache the content in Utils.CachePath
                new BuildTaskStep_ModifyCachedSources(), // Modify the build so it's ready to be uploaded (Remove/add files)
                new BuildTaskStep_PrepareDestinations(), // Make sure the destination is ready to receive the content
                new BuildTaskStep_Upload() // Upload cached content
            };
            
            for (int i = 0; i < steps.Length; i++)
            {
                ProgressUtils.Report(progressId, (float)i/(steps.Length+1), "Upload Builds");
                report.SetProcess(ABuildTask_Step.StepProcess.Intra);
                Task<bool> task = steps[i].Run(this, report);
                while (!task.IsCompleted)
                {
                    tick?.Invoke();
                    await Task.Delay(10);
                }
                
                report.SetProcess(ABuildTask_Step.StepProcess.Post);
                steps[i].PostRunResult(this, report);

                if (!task.Result)
                {
                    break;
                }
            }
            
            report.SetProcess(ABuildTask_Step.StepProcess.Intra);
            BuildTaskReport.StepResult beginCleanupResult = report.NewReport(ABuildTask_Step.StepType.Cleanup);
            
            // Cleanup to make sure nothing is left behind - dirtying up the users computer
            ProgressUtils.Report(progressId, (float)steps.Length/(steps.Length+1), "Cleaning up");
            if (Preferences.DeleteCacheAfterBuild)
            {
                // Delete cache
                int cleanupProgressId = ProgressUtils.Start("Cleanup", "Deleting cached files");
                for (var i = 0; i < cachedLocations.Length; i++)
                {
                    var cachedLocation = cachedLocations[i];
                    if (string.IsNullOrEmpty(cachedLocation))
                    {
                        continue;
                    }
                    
                    if (!Directory.Exists(cachedLocation))
                    {
                        beginCleanupResult.AddLog("Cached location does not exist to cleanup: " + cachedLocation);
                        continue;
                    }
                    
                    await Task.Yield();
                    ProgressUtils.Report(cleanupProgressId, 0, $"Deleting cached files " + (i+1) + "/" + cachedLocations.Length);
                    
                    beginCleanupResult.AddLog("Deleting cached files " + cachedLocation);
                    Directory.Delete(cachedLocation, true);
                }

                // Cleanup configs
                ProgressUtils.Report(cleanupProgressId, 0.5f, "Cleaning up configs");
                BuildTaskReport.StepResult[] cleanupReports = report.NewReports(ABuildTask_Step.StepType.Cleanup, buildConfigs.Count);
                for (int i = 0; i < buildConfigs.Count; i++)
                {
                    var buildConfig = buildConfigs[i];
                    var cleanupResult = cleanupReports[i];
                    if (!buildConfig.Enabled)
                    {
                        cleanupResult.AddLog("Skipping config cleanup because it's disabled");
                        continue;
                    }
                    
                    await Task.Yield();
                    ProgressUtils.Report(cleanupProgressId, 0.5f, $"Cleaning up configs " + (i+1) + "/" + buildConfigs.Count);
                    
                    buildConfig.CleanUp(cleanupResult);
                }
                
                ProgressUtils.Remove(cleanupProgressId);
            }
            else
            {
                beginCleanupResult.AddLog("Skipping deleting cache. Re-enable in preferences.");
            }
            
            ProgressUtils.Remove(progressId);
            report.Complete();
            onComplete?.Invoke(report.Successful);
        }
        
        public void AddConfig(BuildConfig config)
        {
            if (config == null)
            {
                return;
            }
            
            buildConfigs.Add(config);
        }
        
        public void SetBuildDescription(string description)
        {
            buildDescription = description;
        }
    }
}