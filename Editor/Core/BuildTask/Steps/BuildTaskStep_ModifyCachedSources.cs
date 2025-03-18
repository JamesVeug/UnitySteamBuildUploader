using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal class BuildTaskStep_ModifyCachedSources : ABuildTask_Step
    {
        public override string Name => "Modify Cached Sources";
        
        private UploadResult m_lastFailedResult;
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = ModifyBuild(buildTask, j);
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

        private async Task<bool> ModifyBuild(BuildTask task, int sourceIndex)
        {
            BuildConfig buildConfig = task.BuildConfigs[sourceIndex];
            foreach (ABuildConfigModifer modifer in buildConfig.Modifiers)
            {
                UploadResult result = await modifer.ModifyBuildAtPath(task.CachedLocations[sourceIndex], buildConfig, sourceIndex);
                if (!result.Successful)
                {
                    m_lastFailedResult = result;
                    return false;
                }
            }

            return true;
        }

        public override void Failed(BuildTask buildTask)
        {
            string message = "Failed to Modify Cache Sources!";
            message += "\n\n" + m_lastFailedResult.FailReason;
            message += "\n\nSee logs for more info.";
            
            buildTask.DisplayDialog(message, "Okay");
        }
    }
}