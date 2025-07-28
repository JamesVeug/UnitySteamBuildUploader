using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTaskStep_PrepareDestinations : AUploadTask_Step
    {
        public UploadTaskStep_PrepareDestinations(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.PrepareDestinations;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<UploadConfig> buildConfigs = uploadTask.BuildConfigs;
            
            var tasks = new List<Tuple<List<UploadConfig.DestinationData>, Task<bool>>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                string cachePath = uploadTask.CachedLocations[j];
                Task<bool> task = PrepareDestination(buildConfigs[j], cachePath, uploadTask.BuildDescription, report);
                List<UploadConfig.DestinationData> destinations = buildConfigs[j].Destinations.Where(a => a.Enabled).ToList();
                tasks.Add(new Tuple<List<UploadConfig.DestinationData>, Task<bool>>(destinations, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                int totalDestinations = 0;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<List<UploadConfig.DestinationData>, Task<bool>> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        foreach (UploadConfig.DestinationData data in tuple.Item1)
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
                await Task.Yield();
            }
            
            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }
        
        private async Task<bool> PrepareDestination(UploadConfig uploadConfig, string cachePath, string desc, UploadTaskReport report)
        {
            UploadTaskReport.StepResult[] reports = report.NewReports(Type, uploadConfig.Destinations.Count);
            for (var i = 0; i < uploadConfig.Destinations.Count; i++)
            {
                var destination = uploadConfig.Destinations[i];
                var result = reports[i];
                if (!destination.Enabled)
                {
                    result.AddLog("Skipping destination because it's disabled");
                    continue;
                }

                
                AUploadDestination uploadDestination = destination.Destination;
                try
                {
                    bool success = await uploadDestination.Prepare(cachePath, desc, result);
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

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            // Do nothing
            return Task.FromResult(true);
        }
    }
}