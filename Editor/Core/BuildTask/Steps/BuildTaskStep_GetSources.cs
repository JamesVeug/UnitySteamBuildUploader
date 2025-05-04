using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTaskStep_GetSources : ABuildTask_Step
    {
        public override StepType Type => StepType.GetSources;

        public override async Task<bool> Run(BuildTask buildTask, BuildTaskReport report)
        {
            int progressId = ProgressUtils.Start(Type.ToString(), "Setting up...");
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            
            List<Tuple<List<BuildConfig.SourceData>, Task<bool>>> tasks = new();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = GetSources(buildConfigs[j], report);
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
                await Task.Yield();
            }

            ProgressUtils.Remove(progressId);
            return allSuccessful;
        }

        private async Task<bool> GetSources(BuildConfig buildConfig, BuildTaskReport report)
        {
            BuildTaskReport.StepResult[] results = report.NewReports(Type, buildConfig.Sources.Count);
            for (var i = 0; i < buildConfig.Sources.Count; i++)
            {
                var sourceData = buildConfig.Sources[i];
                var result = results[i];
                if (!sourceData.Enabled)
                {
                    result.AddLog("Source skipped - not enabled");
                    continue;
                }

                try
                {
                    bool success = await sourceData.Source.GetSource(buildConfig, result);
                    if (!success)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    result.AddException(e);
                    result.SetFailed("Source failed - " + e.Message);
                    return false;
                }
            }

            return true;
        }

        public override Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            for (var i = 0; i < buildConfigs.Count; i++)
            {
                var config = buildConfigs[i];
                if (!config.Enabled)
                {
                    continue;
                }

                BuildTaskReport.StepResult[] results = report.NewReports(Type, config.Sources.Count);
                for (var j = 0; j < config.Sources.Count; j++)
                {
                    if (!config.Sources[j].Enabled)
                    {
                        continue;
                    }

                    string path = config.Sources[j].Source.SourceFilePath();
                    try{
                        ReportFilesAtPath(path, $"FinalSource: {path}", results[j]);
                    }
                    catch (Exception e)
                    {
                        results[j].AddException(e);
                        results[j].SetFailed("Source failed - " + e.Message);
                        return Task.FromResult(false);
                    }
                }
            }
            return Task.FromResult(true);
        }
    }
}