using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal class BuildTaskStep_PrepareDestinations : ABuildTask_Step
    {
        public override string Name => "Prepare Destinations";
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start("Build Uploader Window", Name);
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<ABuildDestination, Task<bool>>> tasks = new List<Tuple<ABuildDestination, Task<bool>>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                ABuildDestination buildSource = buildConfigs[j].Destination();
                Task<bool> task = buildSource.Prepare();
                tasks.Add(new Tuple<ABuildDestination, Task<bool>>(buildSource, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<ABuildDestination, Task<bool>> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        completionAmount += 0; // TODO: Add progress rate
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
                ProgressUtils.Report(progressId, progress, Name);
                await Task.Delay(10);
            }
            
            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to prepare Destinations! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}