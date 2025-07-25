using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTask
    {
        public List<BuildConfig> BuildConfigs => buildConfigs;
        public string BuildDescription => buildDescription;
        public string[] CachedLocations => cachedLocations;
        
        private List<BuildConfig> buildConfigs;
        private List<BuildConfig.PostUploadActionData> postUploadActions;
        private StringFormatter.Context context;
        private string[] cachedLocations;
        private int progressId;
        private string buildDescription;

        public BuildTask(List<BuildConfig> buildConfigs, string buildDescription) : this(buildConfigs, buildDescription, null)
        {

        }

        public BuildTask(List<BuildConfig> buildConfigs, string buildDescription, List<BuildConfig.PostUploadActionData> postUploadActions)
        {
            this.buildDescription = buildDescription;
            this.buildConfigs = buildConfigs;
            this.postUploadActions = postUploadActions ?? new List<BuildConfig.PostUploadActionData>();
            
            context = new StringFormatter.Context();
            context.TaskDescription = ()=>buildDescription;
        }
        
        public BuildTask()
        {
            buildDescription = "";
            buildConfigs = new List<BuildConfig>();
            postUploadActions = new List<BuildConfig.PostUploadActionData>();
        }

        ~BuildTask()
        {
            if (ProgressUtils.Exists(progressId))
            {
                ProgressUtils.Remove(progressId);
            }
        }

        public async Task Start(BuildTaskReport report, Action<bool> onComplete = null)
        {
            progressId = ProgressUtils.Start("Build Uploader Window", "Upload Builds");
            cachedLocations = new string[buildConfigs.Count];

            ABuildTask_Step[] steps = new ABuildTask_Step[]
            {
                new BuildTaskStep_GetSources(context), // Download content from services or get local folder
                new BuildTaskStep_CacheSources(context), // Cache the content in Utils.CachePath
                new BuildTaskStep_ModifyCachedSources(context), // Modify the build so it's ready to be uploaded (Remove/add files)
                new BuildTaskStep_PrepareDestinations(context), // Make sure the destination is ready to receive the content
                new BuildTaskStep_Upload(context) // Upload cached content
            };
            
            for (int i = 0; i < steps.Length; i++)
            {
                ProgressUtils.Report(progressId, (float)i/(steps.Length+1), "Upload Builds");
                report.SetProcess(ABuildTask_Step.StepProcess.Intra);
                bool stepSuccessful = await steps[i].Run(this, report);
                
                report.SetProcess(ABuildTask_Step.StepProcess.Post);
                bool postStepSuccessful = await steps[i].PostRunResult(this, report);
                if (!stepSuccessful || !postStepSuccessful)
                {
                    break;
                }
            }
            
            await PostUpload_Step(report, steps);
            
            await Cleanup_Step(report, steps);

            ProgressUtils.Remove(progressId);
            report.Complete();
            onComplete?.Invoke(report.Successful);
        }

        private async Task PostUpload_Step(BuildTaskReport report, ABuildTask_Step[] steps)
        {
            report.SetProcess(ABuildTask_Step.StepProcess.Intra);
            BuildTaskReport.StepResult actionResult = report.NewReport(ABuildTask_Step.StepType.PostUpload);
            
            // Cleanup to make sure nothing is left behind - dirtying up the user's computer
            ProgressUtils.Report(progressId, (float)steps.Length/(steps.Length+2), "Post Upload Actions");
            
            int cleanupProgressId = ProgressUtils.Start("Post Upload", "Executing Post Upload Actions");
            for (var i = 0; i < postUploadActions.Count; i++)
            {
                BuildConfig.PostUploadActionData actionData = postUploadActions[i];
                if (actionData == null || actionData.BuildAction == null)
                {
                    actionResult.AddLog("Skipping post upload action because it's null");
                    continue;
                }

                BuildConfig.PostUploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == BuildConfig.PostUploadActionData.UploadCompleteStatus.Never ||
                    (status == BuildConfig.PostUploadActionData.UploadCompleteStatus.Successful && !report.Successful) ||
                    (status == BuildConfig.PostUploadActionData.UploadCompleteStatus.Failed && report.Successful))
                {
                    actionResult.AddLog($"Skipping post upload action {i+1} because it doesn't match the current status");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(cleanupProgressId, 0, $"Executing action " + (i+1) + "/" + postUploadActions.Count);
                    
                actionResult.AddLog($"Executing post upload action: {i+1}");

                bool prepared = await actionData.BuildAction.Prepare(report.Successful, buildDescription, actionResult);
                if (!prepared)
                {
                    actionResult.AddError($"Failed to prepare post upload action: {actionData.BuildAction.GetType().Name}");
                    continue;
                }

                try
                {
                    await actionData.BuildAction.Execute(actionResult, context);
                }
                catch (Exception e)
                {
                    actionResult.AddException(e);
                    continue;
                }
            }
        }

        private async Task Cleanup_Step(BuildTaskReport report, ABuildTask_Step[] steps)
        {
            report.SetProcess(ABuildTask_Step.StepProcess.Intra);
            BuildTaskReport.StepResult beginCleanupResult = report.NewReport(ABuildTask_Step.StepType.Cleanup);
            
            // Cleanup to make sure nothing is left behind - dirtying up the user's computer
            ProgressUtils.Report(progressId, (float)steps.Length/(steps.Length+2), "Cleaning up");
            if (Preferences.DeleteCacheAfterUpload)
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
        }

        public void AddConfig(BuildConfig config)
        {
            if (config == null)
            {
                return;
            }
            
            buildConfigs.Add(config);
        }
        
        public void AddPostUploadAction(BuildConfig.PostUploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            postUploadActions.Add(action);
        }
        
        public void SetBuildDescription(string description)
        {
            buildDescription = description;
        }
    }
}