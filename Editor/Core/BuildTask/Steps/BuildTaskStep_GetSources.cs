using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal class BuildTaskStep_GetSources : ABuildTask_Step
    {
        public override string Name => "Get Sources";

        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<ABuildSource, Task<bool>>> tasks = new List<Tuple<ABuildSource, Task<bool>>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                ABuildSource buildSource = buildConfigs[j].Source();
                Task<bool> task = buildSource.GetSource(buildConfigs[j]);
                tasks.Add(new Tuple<ABuildSource, Task<bool>>(buildSource, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<ABuildSource, Task<bool>> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        completionAmount += tuple.Item1.DownloadProgress();
                        break;
                    }
                    else
                    {
                        allSuccessful &= tuple.Item2.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / tasks.Count;
                ProgressUtils.Report(progressId, progress, "Waiting for sources...");
                await Task.Delay(10);
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }
        
        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to get Sources! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}