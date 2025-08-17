using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTaskStep_ModifyCachedSources : AUploadTask_Step
    {
        public UploadTaskStep_ModifyCachedSources(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.ModifyCacheSources;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<UploadConfig> buildConfigs = uploadTask.UploadConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = ModifyBuild(uploadTask, j, report, m_context);
                tasks.Add(task);
            }

            if (tasks.Count == 0)
            {
                UploadTaskReport.StepResult result = report.NewReport(Type);
                result.AddLog("No modifiers");
                result.SetPercentComplete(1f);
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Task<bool> task = tasks[j];
                    if (!task.IsCompleted)
                    {
                        done = false;
                    }
                    else
                    {
                        allSuccessful &= task.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / tasks.Count;
                ProgressUtils.Report(progressId, progress, "Waiting for all sources to be modified...");
                await Task.Yield();
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> ModifyBuild(UploadTask task, int sourceIndex, UploadTaskReport report, StringFormatter.Context ctx)
        {
            UploadConfig uploadConfig = task.UploadConfigs[sourceIndex];
            UploadTaskReport.StepResult[] results = report.NewReports(Type, uploadConfig.Modifiers.Count);
            for (var i = 0; i < uploadConfig.Modifiers.Count; i++)
            {
                var modifer = uploadConfig.Modifiers[i];
                if (!modifer.Enabled)
                {
                    continue;
                }
                
                var stepResult = results[i];
                try
                {
                    bool success = await modifer.Modifier.ModifyBuildAtPath(task.CachedLocations[sourceIndex], uploadConfig, sourceIndex, stepResult, ctx);
                    if (!success)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    stepResult.AddException(ex);
                    stepResult.SetFailed("Modifier failed: " + modifer.Modifier.GetType().Name);
                    return false;
                }
                finally
                {
                    stepResult.SetPercentComplete(1f);
                }
            }

            return true;
        }
        
        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}