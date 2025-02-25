using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildTask
    {
        private List<BuildConfig> buildConfigs;
        private int progressId;
        private string buildDescription;

        public BuildTask(List<BuildConfig> buildConfigs, string buildDescription)
        {
            this.buildConfigs = buildConfigs;
            this.buildDescription = buildDescription;
        }

        ~BuildTask()
        {
            if (Progress.Exists(progressId))
            {
                Progress.Remove(progressId);
            }
        }

        public async Task Start(Action tick = null)
        {
            this.progressId = Progress.Start("Build Uploader Window", "Getting Sources...");
            
            // Get all the files we need from all configs first
            // Ensure all files are retrieved correctly
            // If any failed to be retrieved, stop the process
            Task<bool> getSourcesTask = GetSources();
            while (!getSourcesTask.IsCompleted)
            {
                tick?.Invoke();
                await Task.Delay(10);
            }

            if (getSourcesTask.Result)
            {
                // We retrieved all the files we need to upload
                // Now we need to make sure we can upload to every config
                Progress.Report(progressId, 0.25f, "Preparing Destinations...");
                Task<bool> prepareTask = PrepareDestinations();
                while (!prepareTask.IsCompleted)
                {
                    tick?.Invoke();
                    await Task.Delay(10);
                }
                
                if (prepareTask.Result)
                {
                    // Builds ready to be uploaded and destinations are prepared
                    // Start uploading every build to their assigned destinations.
                    Progress.Report(progressId, 0.33f, "Uploading...");

                    Task<bool> uploadTask = Upload();
                    while (!uploadTask.IsCompleted)
                    {
                        tick?.Invoke();
                        await Task.Delay(10);
                    }
                    
                    if (uploadTask.Result)
                    {
                        DisplayDialog("Uploads completed successfully!", "Yay!");
                    }
                    else
                    {
                        DisplayDialog("Failed to upload builds! \n\nSee logs for more info.", "Aw");
                    }
                }
                else
                {
                    DisplayDialog("Failed to prepare Destinations! Not uploading any builds.\n\nSee logs for more info.", "Okay");
                }
                
                // Each upload config has attempted to upload
                // Cleanup to make sure nothing is left behind - dirtying up the users computer
                for (int i = 0; i < buildConfigs.Count; i++)
                {
                    Progress.Report(progressId, 0.66f, "Cleaning up...");
                    for (int j = 0; j < buildConfigs.Count; j++)
                    {
                        if (buildConfigs[j].Enabled)
                        {
                            buildConfigs[j].Source().CleanUp();
                            buildConfigs[j].Destination().CleanUp();
                        }
                    }
                }
            }
            else
            {
                DisplayDialog("Failed to get Sources! Not uploading any builds.\n\nSee logs for more info.", "Okay");
            }

            Progress.Remove(progressId);
        }

        private async Task<bool> PrepareDestinations()
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (buildConfigs[i].Enabled)
                {
                    tasks.Add(buildConfigs[i].Destination().Prepare());
                }
            }
            
            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    if (!tasks[j].IsCompleted)
                    {
                        done = false;
                        break;
                    }
                    else
                    {
                        allSuccessful &= tasks[j].Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / tasks.Count;
                Progress.Report(progressId, progress, "Preparing Destinations");
                await Task.Delay(10);
            }

            return allSuccessful;
        }

        private void DisplayDialog(string message, string buttonText)
        {
            EditorUtility.DisplayDialog("Build Uploader", message, buttonText);
        }

        private async Task<bool> GetSources()
        {
            int sourceID = Progress.Start("Get Sources", "Starting...");

            List<Tuple<ABuildSource, Task<bool>>> tasks = new List<Tuple<ABuildSource, Task<bool>>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                ABuildSource buildSource = buildConfigs[j].Source();
                Task<bool> task = buildSource.GetSource();
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
                Progress.Report(sourceID, progress, "Getting Sources");
                await Task.Delay(10);

            }

            for (var i = 0; i < buildConfigs.Count; i++)
            {
                var build = buildConfigs[i];
                if (!build.Enabled)
                {
                    continue;
                }
                
                ABuildSource source = build.Source();
                string path = source.SourceFilePath();
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    Debug.LogError($"Build {i+1} failed to get source. Path does not exist: " + path);
                    break;
                }
            }

            Progress.Remove(sourceID);
            return allSuccessful;
        }

        private async Task<bool> Upload()
        {
            int uploadID = Progress.Start("Uploading", "Starting...");

            bool allSuccessful = true;
            int totalBuilds = GetEnabledBuildCount();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (!buildConfigs[i].Enabled)
                {
                    continue;
                }

                ABuildSource buildSource = buildConfigs[i].Source();
                string description = GetBuildDescription(buildSource);
                string sourceFilePath = buildSource.SourceFilePath();

                ABuildDestination destination = buildConfigs[i].Destination();
                Progress.Report(uploadID, (float)i/totalBuilds, $"Uploading {i+1}/{buildConfigs.Count}");
                
                int destinationID = Progress.Start(destination.ProgressTitle(), destination.ProgressDescription());
                Task<bool> upload = destination.Upload(sourceFilePath, description);
                while (!upload.IsCompleted)
                {
                    await Task.Delay(10);
                    Progress.Report(destinationID, destination.UploadProgress(), destination.ProgressDescription());
                }
                allSuccessful &= upload.Result;
                Progress.Remove(destinationID);
                
                Debug.Log("Uploaded to destination complete: " + i);
            }

            Progress.Remove(uploadID);
            Debug.Log("Upload Complete!");
            return allSuccessful;
        }

        private int GetEnabledBuildCount()
        {
            int completionAmount = 0;
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (buildConfigs[j].Enabled)
                {
                    completionAmount++;
                }
            }

            return completionAmount;
        }

        public string GetBuildDescription(ABuildSource source)
        {
            string description = source.GetBuildDescription();
            if (string.IsNullOrEmpty(description))
            {
                description = buildDescription;
            }
            else
            {
                description += " - " + buildDescription;
            }

            return description;
        }
    }
}