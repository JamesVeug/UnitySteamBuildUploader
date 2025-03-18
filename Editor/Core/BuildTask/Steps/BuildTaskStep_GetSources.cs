using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    internal class BuildTaskStep_GetSources : ABuildTask_Step
    {
        public override string Name => "Get Sources";

        public override async Task<bool> Run(BuildTask buildTask)
        {
            int progressId = ProgressUtils.Start(Name, "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<List<BuildConfig.SourceData>, Task<bool>>> tasks = new();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = GetSources(buildConfigs[j]);
                List<BuildConfig.SourceData> activeSources = buildConfigs[j].Sources.Where(a=>a.Enabled).ToList();
                tasks.Add(new Tuple<List<BuildConfig.SourceData>, Task<bool>>(activeSources, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                int totalSources = 0;
                float completionAmount = 0.0f;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<List<BuildConfig.SourceData>, Task<bool>> tuple = tasks[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                        foreach (BuildConfig.SourceData data in tuple.Item1)
                        {
                            completionAmount += data.Source.DownloadProgress();
                        }
                        totalSources += tuple.Item1.Count;
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

                float progress = completionAmount / totalSources;
                ProgressUtils.Report(progressId, progress, "Waiting for sources...");
                await Task.Delay(10);
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> GetSources(BuildConfig buildConfig)
        {
            foreach (BuildConfig.SourceData sourceData in buildConfig.Sources)
            {
                if (!sourceData.Enabled)
                {
                    continue;
                }
                
                bool success = await sourceData.Source.GetSource(buildConfig);
                if (!success)
                {
                    return false;
                }
            }
            
            return true;
        }

        public override void Failed(BuildTask buildTask)
        {
            buildTask.DisplayDialog("Failed to get Sources! Not uploading any builds.\n\nSee logs for more info.", "Okay");
        }
    }
}