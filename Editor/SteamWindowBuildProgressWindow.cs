using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class SteamWindowBuildProgressWindow
    {
        private List<BuildConfig> steamBuilds;
        private int progressId;
        private string buildDescription;

        public SteamWindowBuildProgressWindow(List<BuildConfig> steamBuilds, string buildDescription)
        {
            this.steamBuilds = steamBuilds;
            this.buildDescription = buildDescription;
        }

        ~SteamWindowBuildProgressWindow()
        {
            if (Progress.Exists(progressId))
            {
                Progress.Remove(progressId);
            }
        }

        public async Task StartProgress(Action tick = null)
        {
            this.progressId = Progress.Start("Steam Build Window", "Getting Sources...");
            
            Task<bool> getSourcesTask = GetSources();
            while (!getSourcesTask.IsCompleted)
            {
                tick?.Invoke();
                await Task.Delay(10);
            }

            if (getSourcesTask.Result)
            {
                Progress.Report(progressId, 0.25f, "Preparing Destinations...");
                Task<bool> prepareTask = PrepareDestinations();
                while (!prepareTask.IsCompleted)
                {
                    tick?.Invoke();
                    await Task.Delay(10);
                }
                
                if (prepareTask.Result)
                {
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
                
                for (int i = 0; i < steamBuilds.Count; i++)
                {
                    Progress.Report(progressId, 0.66f, "Cleaning up...");
                    for (int j = 0; j < steamBuilds.Count; j++)
                    {
                        if (steamBuilds[j].Enabled)
                        {
                            steamBuilds[j].Source().CleanUp();
                            steamBuilds[j].Destination().CleanUp();
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
            for (int i = 0; i < steamBuilds.Count; i++)
            {
                if (steamBuilds[i].Enabled)
                {
                    tasks.Add(steamBuilds[i].Destination().Prepare());
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
            EditorUtility.DisplayDialog("Steam Build Uploader", message, buttonText);
        }

        private async Task<bool> GetSources()
        {
            int sourceID = Progress.Start("Get Sources", "Starting...");

            List<Tuple<ASteamBuildSource, Task<bool>>> tasks = new List<Tuple<ASteamBuildSource, Task<bool>>>();
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (!steamBuilds[j].Enabled)
                {
                    continue;
                }

                ASteamBuildSource buildSource = steamBuilds[j].Source();
                Task<bool> task = buildSource.GetSource();
                tasks.Add(new Tuple<ASteamBuildSource, Task<bool>>(buildSource, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<ASteamBuildSource, Task<bool>> tuple = tasks[j];
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

            bool allPathsExist = true;
            for (var i = 0; i < steamBuilds.Count; i++)
            {
                var build = steamBuilds[i];
                if (!build.Enabled)
                {
                    continue;
                }
                
                ASteamBuildSource source = build.Source();
                string path = source.SourceFilePath();
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    allPathsExist = false;
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
            for (int i = 0; i < steamBuilds.Count; i++)
            {
                if (!steamBuilds[i].Enabled)
                {
                    continue;
                }

                ASteamBuildSource buildSource = steamBuilds[i].Source();
                string description = GetBuildDescription(buildSource);
                string sourceFilePath = buildSource.SourceFilePath();

                ASteamBuildDestination destination = steamBuilds[i].Destination();
                Progress.Report(uploadID, (float)i/totalBuilds, $"Uploading {i+1}/{steamBuilds.Count}");
                
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
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (steamBuilds[j].Enabled)
                {
                    completionAmount++;
                }
            }

            return completionAmount;
        }

        public string GetBuildDescription(ASteamBuildSource source)
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