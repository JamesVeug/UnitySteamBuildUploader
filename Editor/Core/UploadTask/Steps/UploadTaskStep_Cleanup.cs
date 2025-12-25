using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public UploadTaskStep_Cleanup(Context context) : base(context)
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
            for (int i = 0; i < uploadTask.UploadConfigs.Count; i++)
            {
                m_stateResults.Add(new StateResult(uploadTask.UploadConfigs[i], configResults[i], (index) => "Config " + (index + 1)));
            }
            
            
            int activeConfigIndex = 0;
            for (int i = 0; i < uploadTask.UploadConfigs.Count; i++)
            {
                UploadConfig config = uploadTask.UploadConfigs[i];
                UploadTaskReport.StepResult cleanupResult = configResults[i];
                if (!config.Enabled)
                {
                    cleanupResult.SetSkipped("Skipping config cleanup because it's disabled");
                    continue;
                }
                
                await Task.Yield();
                ProgressUtils.Report(cleanupProgressId, 0.33f, $"Cleaning up configs " + (i+1) + "/" + uploadTask.UploadConfigs.Count);
                
                await config.CleanUp(activeConfigIndex++, config, cleanupResult);
                cleanupResult.SetPercentComplete(1f);
            }
            
            // Cleanup actions
            ProgressUtils.Report(cleanupProgressId, 0.66f, "Cleaning up Upload configs");
            UploadTaskReport.StepResult[] actionResults = report.NewReports(StepType.Cleanup, uploadTask.Actions.Count);
            for (int i = 0; i < uploadTask.Actions.Count; i++)
            {
                UploadConfig.UploadActionData actionData = uploadTask.Actions[i];
                UploadTaskReport.StepResult cleanupResult = actionResults[i];
                if (actionData.UploadAction == null)
                {
                    cleanupResult.SetSkipped("Skipping post action cleanup because it's null");
                    continue;
                }

                if (actionData.WhenToExecute == UploadConfig.UploadActionData.UploadCompleteStatus.Never)
                {
                    cleanupResult.SetSkipped("Skipping config cleanup because it's Execute condition is set to Never");
                    continue;
                }

                if (actionData.Triggers.Count(a=>a != UploadConfig.UploadActionData.UploadTrigger.Never) == 0)
                {
                    cleanupResult.SetSkipped("Skipping config cleanup because it has no valid Triggers");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(cleanupProgressId, 0.66f, $"Cleaning up post action " + (i+1) + "/" + uploadTask.Actions.Count);
                
                await actionData.UploadAction.CleanUp(cleanupResult);
                cleanupResult.SetPercentComplete(1f);
            }
            
            ProgressUtils.Remove(cleanupProgressId);
            
            beginCleanupResult.SetPercentComplete(1f);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report,
            bool allStepsSuccessful)
        {
            return Task.FromResult(true);
        }
    }
}