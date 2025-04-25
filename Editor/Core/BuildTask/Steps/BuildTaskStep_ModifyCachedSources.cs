using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTaskStep_ModifyCachedSources : ABuildTask_Step
    {
        public override StepType Type => StepType.ModifyCacheSources;
        
        public override async Task<bool> Run(BuildTask buildTask, BuildTaskReport report)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = ModifyBuild(buildTask, j, report);
                tasks.Add(task);
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
                await Task.Delay(10);
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> ModifyBuild(BuildTask task, int sourceIndex, BuildTaskReport report)
        {
            BuildConfig buildConfig = task.BuildConfigs[sourceIndex];
            BuildTaskReport.StepResult[] results = report.NewReports(Type, buildConfig.Modifiers.Count);
            for (var i = 0; i < buildConfig.Modifiers.Count; i++)
            {
                var modifer = buildConfig.Modifiers[i];
                var stepResult = results[i];
                bool success = await modifer.Modifier.ModifyBuildAtPath(task.CachedLocations[sourceIndex], buildConfig, sourceIndex, stepResult);
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }
        
        public override Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            ReportCachedFiles(buildTask, report);
            return Task.FromResult(true);
        }
    }
}