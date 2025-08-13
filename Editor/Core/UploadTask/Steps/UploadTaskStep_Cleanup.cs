using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTaskStep_Cleanup : AUploadTask_Step
    {
        public UploadTaskStep_Cleanup(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.Cleanup;
        public override bool RequiresEverythingBeforeToSucceed => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report)
        {
            report.SetProcess(StepProcess.Intra);
            UploadTaskReport.StepResult beginCleanupResult = report.NewReport(StepType.Cleanup);
            
            // Cleanup to make sure nothing is left behind - dirtying up the user's computer
            if (Preferences.DeleteCacheAfterUpload)
            {
                // Delete cache
                int cleanupProgressId = ProgressUtils.Start("Cleanup", "Deleting cached files");
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

                // Cleanup configs
                ProgressUtils.Report(cleanupProgressId, 0.5f, "Cleaning up Upload configs");
                UploadTaskReport.StepResult[] cleanupReports = report.NewReports(StepType.Cleanup, uploadTask.UploadConfigs.Count);
                for (int i = 0; i < uploadTask.UploadConfigs.Count; i++)
                {
                    var buildConfig = uploadTask.UploadConfigs[i];
                    var cleanupResult = cleanupReports[i];
                    if (!buildConfig.Enabled)
                    {
                        cleanupResult.AddLog("Skipping config cleanup because it's disabled");
                        cleanupResult.SetPercentComplete(1f);
                        continue;
                    }
                    
                    await Task.Yield();
                    ProgressUtils.Report(cleanupProgressId, 0.5f, $"Cleaning up configs " + (i+1) + "/" + uploadTask.UploadConfigs.Count);
                    
                    buildConfig.CleanUp(cleanupResult);
                    cleanupResult.SetPercentComplete(1f);
                }
                
                ProgressUtils.Remove(cleanupProgressId);
            }
            else
            {
                beginCleanupResult.AddLog("Skipping deleting cache. Re-enable in preferences.");
            }
            
            beginCleanupResult.SetPercentComplete(1f);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}