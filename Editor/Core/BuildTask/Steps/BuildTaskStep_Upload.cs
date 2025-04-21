using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    public class BuildTaskStep_Upload : ABuildTask_Step
    {
        public override string Name => "Upload";
        
        private class UploadResultData
        {
            public BuildConfig Config;
            public UploadResult[] Results;
        }
        
        private UploadResultData[] m_configUploadResults;
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            m_configUploadResults = new UploadResultData[buildConfigs.Count];

            List<Tuple<List<BuildConfig.DestinationData>, Task<bool>>> uploads = new();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (!buildConfigs[i].Enabled)
                {
                    continue;
                }
                List<BuildConfig.DestinationData> destinations = buildConfigs[i].Destinations.Where(a=>a.Enabled).ToList();

                UploadResultData data = new UploadResultData();
                data.Config = buildConfigs[i];
                data.Results = new UploadResult[destinations.Count];
                m_configUploadResults[i] = data;
                
                Task<bool> upload = Upload(buildTask, i, data, destinations);
                uploads.Add(new (destinations, upload));
            }
            
            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                for (int j = 0; j < uploads.Count; j++)
                {
                    var tuple = uploads[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                    }
                    else
                    {
                        allSuccessful &= tuple.Item2.Result;
                    }
                }

                if (done)
                {
                    break;
                }

                await Task.Delay(10);
            }

            return allSuccessful;
        }

        private async Task<bool> Upload(BuildTask buildTask, int configIndex, UploadResultData data, List<BuildConfig.DestinationData> destinations)
        {
            int uploadID = ProgressUtils.Start(Name, $"Uploading Config {configIndex + 1}");

            string sourceFilePath = buildTask.CachedLocations[configIndex];
            List<Task<UploadResult>> uploadTasks = new List<Task<UploadResult>>();
            foreach (BuildConfig.DestinationData destinationData in destinations)
            {
                Task<UploadResult> task = destinationData.Destination.Upload(sourceFilePath, buildTask.BuildDescription);
                uploadTasks.Add(task);
            }
            
            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < uploadTasks.Count; j++)
                {
                    Task<UploadResult> tuple = uploadTasks[j];
                    ABuildDestination destination = destinations[j].Destination;
                    if (!tuple.IsCompleted)
                    {
                        done = false;
                        completionAmount += destination.UploadProgress();
                    }
                    else
                    {
                        allSuccessful &= tuple.Result.Successful;
                        data.Results[j] = tuple.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / destinations.Count;
                ProgressUtils.Report(uploadID, progress, "Waiting for all to be uploaded...");
                await Task.Delay(10);
            }
            
            return allSuccessful;
        }

        public override void Failed(BuildTask buildTask)
        {
            int failedConfigs = m_configUploadResults.Count(a => a.Results.Any(b=>!b.Successful));
            int totalConfigs = m_configUploadResults.Length;
            string message = $"{failedConfigs}/{totalConfigs} Builds Failed to Upload!";
            for (int i = 0; i < m_configUploadResults.Length; i++)
            {
                UploadResultData result = m_configUploadResults[i];
                for (var j = 0; j < result.Results.Length; j++)
                {
                    var uploadResult = result.Results[j];
                    if (!uploadResult.Successful)
                    {
                        message += $"\nBuild #{i + 1} Destination #{j+1} - " + uploadResult.FailReason;
                    }
                }
            }
            
            message += "\n\nSee logs for more info.";


            buildTask.DisplayDialog(message, "Aw");
        }
    }
}