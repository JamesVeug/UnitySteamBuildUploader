using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTaskStep_PrepareDestinations : ABuildTask_Step
    {
        public override StepType Type => StepType.PrepareDestinations;
        
        public override async Task<bool> Run(BuildTask buildTask, BuildTaskReport report)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<List<BuildConfig.DestinationData>, Task<bool>>> tasks = new();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                string cachePath = buildTask.CachedLocations[j];
                Task<bool> task = PrepareDestination(buildConfigs[j], cachePath, buildTask.BuildDescription, report);
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
        
        private async Task<bool> PrepareDestination(BuildConfig buildConfig, string cachePath, string desc, BuildTaskReport report)
        {
            BuildTaskReport.StepResult[] reports = report.NewReports(Type, buildConfig.Destinations.Count);
            for (var i = 0; i < buildConfig.Destinations.Count; i++)
            {
                var destination = buildConfig.Destinations[i];
                var result = reports[i];
                if (!destination.Enabled)
                {
                    result.AddLog("Skipping destination because it's disabled");
                    continue;
                }

                
                ABuildDestination buildDestination = destination.Destination;
                try
                {
                    bool success = await buildDestination.Prepare(cachePath, desc, result);
                    if (!success)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    result.AddException(e);
                    result.SetFailed("Failed to prepare destination: " + e.Message);
                    return false;
                }
            }

            return true;
        }

        public override void PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            // Do nothing
        }
    }
}