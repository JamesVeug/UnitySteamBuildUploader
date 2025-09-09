using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Cleanup step to remove any cached files or data to avoid dirtying up the user's computer
    /// This step always runs as a last step - even if previous steps failed
    /// </summary>
    public class UploadTaskStep_Cleanup : AUploadTask_Step
    {
        public UploadTaskStep_Cleanup(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.Cleanup;
        public override bool RequiresEverythingBeforeToSucceed => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            report.SetProcess(StepProcess.Intra);
            UploadTaskReport.StepResult beginCleanupResult = report.NewReport(StepType.Cleanup);
            
            // Cleanup to make sure nothing is left behind - dirtying up the user's computer
            int cleanupProgressId = ProgressUtils.Start("Cleanup", "Deleting cached files");
            if (Preferences.DeleteCacheAfterUpload)
            {
                // Delete cache
                if (uploadTask.CachedLocations.Length > 0)
                {
                    string parentFolder = Path.GetDirectoryName(uploadTask.CachedLocations[0]);
                    if (!string.IsNullOrEmpty(parentFolder) && Directory.Exists(parentFolder))
                    {
                        beginCleanupResult.AddLog("Deleting cached files in parent folder: " + parentFolder);
                        Directory.Delete(parentFolder, true);
                    }
                    else
                    {
                        beginCleanupResult.AddLog("Parent folder does not exist to cleanup: " + parentFolder);
                    }
                }
            }
            else
            {
                beginCleanupResult.AddLog("Skipping deleting cache. Re-enable in preferences.");
            }

            // Cleanup configs
            ProgressUtils.Report(cleanupProgressId, 0.33f, "Cleaning up Upload configs");
            UploadTaskReport.StepResult[] configResults = report.NewReports(StepType.Cleanup, uploadTask.UploadConfigs.Count);
            int activeConfigIndex = 0;
            for (int i = 0; i < uploadTask.UploadConfigs.Count; i++)
            {
                UploadConfig config = uploadTask.UploadConfigs[i];
                UploadTaskReport.StepResult cleanupResult = configResults[i];
                if (!config.Enabled)
                {
                    cleanupResult.AddLog("Skipping config cleanup because it's disabled");
                    cleanupResult.SetPercentComplete(1f);
                    continue;
                }
                
                await Task.Yield();
                ProgressUtils.Report(cleanupProgressId, 0.33f, $"Cleaning up configs " + (i+1) + "/" + uploadTask.UploadConfigs.Count);
                
                await config.CleanUp(activeConfigIndex++, config, cleanupResult);
                cleanupResult.SetPercentComplete(1f);
            }
            
            // Cleanup post actions
            ProgressUtils.Report(cleanupProgressId, 0.66f, "Cleaning up Upload configs");
            UploadTaskReport.StepResult[] actionResults = report.NewReports(StepType.Cleanup, uploadTask.PostUploadActions.Count);
            for (int i = 0; i < uploadTask.PostUploadActions.Count; i++)
            {
                UploadConfig.PostUploadActionData actionData = uploadTask.PostUploadActions[i];
                UploadTaskReport.StepResult cleanupResult = actionResults[i];
                if (actionData.UploadAction == null)
                {
                    cleanupResult.AddLog("Skipping post action cleanup because it's null");
                    cleanupResult.SetPercentComplete(1f);
                    continue;
                }

                if (actionData.WhenToExecute == UploadConfig.PostUploadActionData.UploadCompleteStatus.Never)
                {
                    cleanupResult.AddLog("Skipping config cleanup because it's set to Never");
                    cleanupResult.SetPercentComplete(1f);
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(cleanupProgressId, 0.66f, $"Cleaning up post action " + (i+1) + "/" + uploadTask.PostUploadActions.Count);
                
                await actionData.UploadAction.CleanUp(cleanupResult);
                cleanupResult.SetPercentComplete(1f);
            }
            
            ProgressUtils.Remove(cleanupProgressId);
            
            beginCleanupResult.SetPercentComplete(1f);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}