using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    internal class BuildTaskStep_Upload : ABuildTask_Step
    {
        public override string Name => "Upload";
        
        private UploadResult[] m_uploadResults;
        
        public override async Task<bool> Run(BuildTask buildTask)
        {
            int uploadID = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            m_uploadResults = new UploadResult[buildConfigs.Count];

            List<Tuple<ABuildDestination, Task<UploadResult>>> uploads = new List<Tuple<ABuildDestination, Task<UploadResult>>>();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (!buildConfigs[i].Enabled)
                {
                    continue;
                }


                string sourceFilePath = buildTask.CachedLocations[i];
                ABuildDestination destination = buildConfigs[i].Destination();
                Task<UploadResult> upload = destination.Upload(sourceFilePath, buildTask.BuildDescription);
                uploads.Add(new Tuple<ABuildDestination, Task<UploadResult>>(destination, upload));
            }
            
            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < uploads.Count; j++)
                {
                    Tuple<ABuildDestination, Task<UploadResult>> tuple = uploads[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        completionAmount += tuple.Item1.UploadProgress();
                    }
                    else
                    {
                        allSuccessful &= tuple.Item2.Result.Successful;
                        m_uploadResults[j] = tuple.Item2.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / uploads.Count;
                ProgressUtils.Report(uploadID, progress, "Waiting for all to be uploaded...");
                await Task.Delay(10);
            }

            ProgressUtils.Remove(uploadID);
            return allSuccessful;
        }

        private int GetEnabledBuildCount(List<BuildConfig> configs)
        {
            int completionAmount = 0;
            for (int j = 0; j < configs.Count; j++)
            {
                if (configs[j].Enabled)
                {
                    completionAmount++;
                }
            }

            return completionAmount;
        }

        private string GetBuildDescription(BuildTask task, ABuildSource source)
        {
            string description = source.GetBuildDescription();
            if (string.IsNullOrEmpty(description))
            {
                description = task.BuildDescription;
            }
            else
            {
                description += " - " + task.BuildDescription;
            }

            return description;
        }

        public override void Failed(BuildTask buildTask)
        {
            int failedCount = m_uploadResults.Count(a => !a.Successful);
            int totalCount = m_uploadResults.Length;
            string message = $"{failedCount}/{totalCount} Builds Failed to Upload!";
            for (int i = 0; i < m_uploadResults.Length; i++)
            {
                UploadResult result = m_uploadResults[i];
                if (!result.Successful)
                {
                    message += $"\nBuild #{i+1} - " + result.FailReason;
                }
            }
            
            message += "\n\nSee logs for more info.";


            buildTask.DisplayDialog(message, "Aw");
        }
    }
}