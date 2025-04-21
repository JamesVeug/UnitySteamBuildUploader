using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTaskStep_PrepareDestinations : ABuildTask_Step
    {
        public override string Name => "Prepare Destinations";
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<List<BuildConfig.DestinationData>, Task<bool>>> tasks = new();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = PrepareDestination(buildConfigs[j]);
                List<BuildConfig.DestinationData> destinations = buildConfigs[j].Destinations.Where(a => a.Enabled).ToList();
                tasks.Add(new Tuple<List<BuildConfig.DestinationData>, Task<bool>>(destinations, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                int totalDestinations = 0;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<List<BuildConfig.DestinationData>, Task<bool>> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        foreach (BuildConfig.DestinationData data in tuple.Item1)
                        {
                            completionAmount += 0.5f; // TODO: Add progress to PrepareDestination
                        }
                        totalDestinations += tuple.Item1.Count;
                    }
                    else
                    {
                        allSuccessful &= tuple.Item2.Result;
                        completionAmount += tuple.Item1.Count;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / totalDestinations;
                ProgressUtils.Report(progressId, progress, "Waiting for destinations to prepare...");
                await Task.Delay(10);
            }
            
            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }
        
        private async Task<bool> PrepareDestination(BuildConfig buildConfig)
        {
            foreach (BuildConfig.DestinationData destination in buildConfig.Destinations)
            {
                if (!destination.Enabled)
                {
                    continue;
                }
                
                ABuildDestination buildDestination = destination.Destination;
                bool success = await buildDestination.Prepare();
                if (!success)
                {
                    return false;
                }
            }
            
            return true;
        }

        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to prepare Destinations! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}