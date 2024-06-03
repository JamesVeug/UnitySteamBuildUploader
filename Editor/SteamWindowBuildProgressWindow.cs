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
            await GetSource(tick);
            await Upload(tick);

            for (int i = 0; i < steamBuilds.Count; i++)
            {
                if (steamBuilds[i].Enabled)
                {
                    steamBuilds[i].Source().CleanUp();
                    steamBuilds[i].Destination().CleanUp();
                }
            }
            Debug.Log("StartProgress complete!");
        }

        private async Task ProcessSource(Task source, AsyncOperation op)
        {
            await source;
            op.Successful = true;
        }

        private async Task GetSource(Action tick = null)
        {
            this.progressId = Progress.Start("Steam Build Window", "Starting...");

            List<AsyncOperation<ASteamBuildSource>> coroutines = new List<AsyncOperation<ASteamBuildSource>>();
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (!steamBuilds[j].Enabled)
                {
                    continue;
                }

                Task source = steamBuilds[j].Source().GetSource();

                AsyncOperation<ASteamBuildSource> op = new AsyncOperation<ASteamBuildSource>();
                op.Data = steamBuilds[j].Source();
                op.SetIterator(ProcessSource(source, op));
                coroutines.Add(op);
            }

            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < coroutines.Count; j++)
                {
                    if (!coroutines[j].Successful)
                    {
                        done = false;
                        completionAmount += coroutines[j].Data.DownloadProgress();
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

                float progress = completionAmount / coroutines.Count;
                Progress.Report(progressId, progress, "Getting Sources");
                tick?.Invoke();
                await Task.Delay(10);
            }

            Progress.Remove(progressId);
        }

        private async Task Upload(Action tick)
        {
            this.progressId = Progress.Start("Uploading", "Starting...");

            int totalBuilds = GetEnabledBuildCount();
            float completionAmount = 0.0f;
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
                await destination.Upload(sourceFilePath, description);
                completionAmount++;
                Debug.Log("Uploaded to destination complete: " + i);

                float progress = completionAmount / totalBuilds;
                Progress.Report(progressId, progress, "Uploading");
                tick?.Invoke();
                await Task.Delay(10);
            }

            Progress.Remove(progressId);
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