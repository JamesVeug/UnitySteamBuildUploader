using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

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

        public IEnumerator StartProgress()
        {
            yield return GetSource();
            yield return Upload();

            for (int i = 0; i < steamBuilds.Count; i++)
            {
                if (steamBuilds[i].Enabled)
                {
                    steamBuilds[i].Source().CleanUp();
                    steamBuilds[i].Destination().CleanUp();
                }
            }
        }

        private IEnumerator ProcessSource(IEnumerator source, AsyncOperation op)
        {
            yield return source;
            op.Successful = true;
        }

        private IEnumerator GetSource()
        {
            this.progressId = Progress.Start("Steam Build Window", "Starting...");

            List<AsyncOperation<ASteamBuildSource>> coroutines = new List<AsyncOperation<ASteamBuildSource>>();
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (!steamBuilds[j].Enabled)
                {
                    continue;
                }

                IEnumerator source = steamBuilds[j].Source().GetSource();

                AsyncOperation<ASteamBuildSource> op = new AsyncOperation<ASteamBuildSource>();
                op.Data = steamBuilds[j].Source();
                op.SetIterator(ProcessSource(source, op));
                EditorCoroutineUtility.StartCoroutine(op, this);
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
                yield return null;
            }

            Progress.Remove(progressId);
        }

        private IEnumerator Upload()
        {
            this.progressId = Progress.Start("Uploading", "Starting...");

            int totalBuilds = GetEnabledBuildCount();
            float completionAmount = 0.0f;
            for (int j = 0; j < steamBuilds.Count; j++)
            {
                if (!steamBuilds[j].Enabled)
                {
                    continue;
                }

                ASteamBuildSource buildSource = steamBuilds[j].Source();
                string description = GetBuildDescription(buildSource);
                string sourceFilePath = buildSource.SourceFilePath();

                ASteamBuildDestination destination = steamBuilds[j].Destination();
                yield return destination.Upload(sourceFilePath, description);
                completionAmount++;

                float progress = completionAmount / totalBuilds;
                Progress.Report(progressId, progress, "Uploading");
                yield return null;
            }

            Progress.Remove(progressId);
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