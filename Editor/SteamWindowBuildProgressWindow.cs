using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamWindowBuildProgressWindow
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
            
            Task task = GetSources();
            while (!task.IsCompleted)
            {
                tick?.Invoke();
                await Task.Delay(10);
            }
            
            Progress.Report(progressId, 0.33f, "Uploading...");
            
            Task uploadTask = Upload();
            while (!uploadTask.IsCompleted)
            {
                tick?.Invoke();
                await Task.Delay(10);
            }

            Progress.Report(progressId, 0.66f, "Cleaning up...");
            for (int i = 0; i < steamBuilds.Count; i++)
            {
                if (steamBuilds[i].Enabled)
                {
                    steamBuilds[i].Source().CleanUp();
                    steamBuilds[i].Destination().CleanUp();
                }
            }
            
            Progress.Remove(progressId);
            Debug.Log("StartProgress complete!");
        }

        private async Task GetSources(Action tick = null)
        {
            int sourceID = Progress.Start("Get Sources", "Starting...");

            List<Tuple<ASteamBuildSource, Task>> tasks = new List<Tuple<ASteamBuildSource, Task>>();
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (!steamBuilds[j].Enabled)
                {
                    continue;
                }

                ASteamBuildSource buildSource = steamBuilds[j].Source();
                Task task = buildSource.GetSource();
                tasks.Add(new Tuple<ASteamBuildSource, Task>(buildSource, task));
            }

            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<ASteamBuildSource,Task> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        completionAmount += tuple.Item1.DownloadProgress();
                        break;
                    }
                    else
                    {
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

            Progress.Remove(sourceID);
        }

        private async Task Upload()
        {
            int uploadID = Progress.Start("Uploading", "Starting...");

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
                Task upload = destination.Upload(sourceFilePath, description);
                while (!upload.IsCompleted)
                {
                    await Task.Delay(10);
                    Progress.Report(destinationID, destination.UploadProgress(), destination.ProgressDescription());
                }
                Progress.Remove(destinationID);
                
                Debug.Log("Uploaded to destination complete: " + i);
            }

            Progress.Remove(uploadID);
            Debug.Log("Upload Complete!");
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