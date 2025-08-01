using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTask
    {
        public List<UploadConfig> BuildConfigs => buildConfigs;
        public string BuildDescription => buildDescription;
        public string[] CachedLocations => cachedLocations;
        
        private List<UploadConfig> buildConfigs;
        private List<UploadConfig.PostUploadActionData> postUploadActions;
        private StringFormatter.Context context;
        private string[] cachedLocations;
        private int progressId;
        private string buildDescription;

        public UploadTask(List<UploadConfig> buildConfigs, string buildDescription) : this(buildConfigs, buildDescription, null)
        {

        }

        public UploadTask(List<UploadConfig> buildConfigs, string buildDescription, List<UploadConfig.PostUploadActionData> postUploadActions)
        {
            this.buildDescription = buildDescription;
            this.buildConfigs = buildConfigs;
            this.postUploadActions = postUploadActions ?? new List<UploadConfig.PostUploadActionData>();
            
            context = new StringFormatter.Context();
            context.TaskDescription = ()=>buildDescription;
        }
        
        public UploadTask()
        {
            buildDescription = "";
            buildConfigs = new List<UploadConfig>();
            postUploadActions = new List<UploadConfig.PostUploadActionData>();
        }

        ~UploadTask()
        {
            if (ProgressUtils.Exists(progressId))
            {
                ProgressUtils.Remove(progressId);
            }
        }

        public async Task Start(UploadTaskReport report, Action<bool> onComplete = null)
        {
            progressId = ProgressUtils.Start("Build Uploader Window", "Upload Builds");
            cachedLocations = new string[buildConfigs.Count];

            AUploadTask_Step[] steps = new AUploadTask_Step[]
            {
                new UploadTaskStep_GetSources(context), // Download content from services or get local folder
                new UploadTaskStep_CacheSources(context), // Cache the content in Utils.CachePath
                new UploadTaskStep_ModifyCachedSources(context), // Modify the build so it's ready to be uploaded (Remove/add files)
                new UploadTaskStep_PrepareDestinations(context), // Make sure the destination is ready to receive the content
                new UploadTaskStep_Upload(context) // Upload cached content
            };
            
            context.UploadTaskFailText = () =>
            {
                if (report.Successful)
                {
                    return "Upload task did not fail.";
                }
                
                var failReasons = report.GetFailReasons();
                if (!failReasons.Any())
                {
                    return "No specific failure reasons provided.";
                }

                string reasonText = "";
                foreach ((AUploadTask_Step.StepType Key, string FailReason) reason in failReasons)
                {
                    if (string.IsNullOrEmpty(reasonText))
                    {
                        reasonText += $"{reason.Key}: {reason.FailReason}";
                    }
                    else
                    {
                        reasonText += $"\n{reason.Key}: {reason.FailReason}";
                    }
                }
                return reasonText;
            };
            
            // Do upload steps
            for (int i = 0; i < steps.Length; i++)
            {
                ProgressUtils.Report(progressId, (float)i/(steps.Length+1), "Upload Builds");
                report.SetProcess(AUploadTask_Step.StepProcess.Intra);
                bool stepSuccessful = await steps[i].Run(this, report);
                
                report.SetProcess(AUploadTask_Step.StepProcess.Post);
                bool postStepSuccessful = await steps[i].PostRunResult(this, report);
                if (!stepSuccessful || !postStepSuccessful)
                {
                    break;
                }
            }
            
            // Do any post upload actions
            await PostUpload_Step(report, steps);
            
            // Cleanup
            await Cleanup_Step(report, steps);

            ProgressUtils.Remove(progressId);
            report.Complete();
            onComplete?.Invoke(report.Successful);
        }

        private async Task PostUpload_Step(UploadTaskReport report, AUploadTask_Step[] steps)
        {
            report.SetProcess(AUploadTask_Step.StepProcess.Intra);
            UploadTaskReport.StepResult actionResult = report.NewReport(AUploadTask_Step.StepType.PostUpload);
            
            // Cleanup to make sure nothing is left behind - dirtying up the user's computer
            ProgressUtils.Report(progressId, (float)steps.Length/(steps.Length+2), "Post Upload Actions");
            
            int postActionID = ProgressUtils.Start("Post Upload", "Executing Post Upload Actions...");
            for (var i = 0; i < postUploadActions.Count; i++)
            {
                UploadConfig.PostUploadActionData actionData = postUploadActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    actionResult.AddLog("Skipping post upload action because it's null");
                    continue;
                }

                UploadConfig.PostUploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.IfSuccessful && !report.Successful) ||
                    (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.IfFailed && report.Successful))
                {
                    actionResult.AddLog($"Skipping post upload action {i+1} because it doesn't match the current status");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(postActionID, 0, $"Executing action " + (i+1) + "/" + postUploadActions.Count);
                    
                actionResult.AddLog($"Executing post upload action: {i+1}");

                bool prepared = await actionData.UploadAction.Prepare(report.Successful, buildDescription, actionResult);
                if (!prepared)
                {
                    actionResult.AddError($"Failed to prepare post upload action: {actionData.UploadAction.GetType().Name}");
                    continue;
                }

                try
                {
                    await actionData.UploadAction.Execute(actionResult, context);
                }
                catch (Exception e)
                {
                    actionResult.AddException(e);
                }
            }
            ProgressUtils.Remove(postActionID);
        }

        private async Task Cleanup_Step(UploadTaskReport report, AUploadTask_Step[] steps)
        {
            report.SetProcess(AUploadTask_Step.StepProcess.Intra);
            UploadTaskReport.StepResult beginCleanupResult = report.NewReport(AUploadTask_Step.StepType.Cleanup);
            
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
                UploadTaskReport.StepResult[] cleanupReports = report.NewReports(AUploadTask_Step.StepType.Cleanup, buildConfigs.Count);
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

        public void AddConfig(UploadConfig config)
        {
            if (config == null)
            {
                return;
            }
            
            buildConfigs.Add(config);
        }
        
        public void AddPostUploadAction(UploadConfig.PostUploadActionData action)
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