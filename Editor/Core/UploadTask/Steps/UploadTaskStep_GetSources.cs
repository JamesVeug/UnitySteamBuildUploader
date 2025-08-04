using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTaskStep_GetSources : AUploadTask_Step
    {
        public UploadTaskStep_GetSources(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.GetSources;

        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report)
        {
            List<UploadConfig> buildConfigs = uploadTask.UploadConfigs;

            var tasks = new List<Tuple<List<UploadConfig.SourceData>, Task<bool>>>();
            for (int j = 0; j < buildConfigs.Count; j++)
            {
                if (!buildConfigs[j].Enabled)
                {
                    continue;
                }

                Task<bool> task = GetSources(buildConfigs[j], report, m_context);
                List<UploadConfig.SourceData> activeSources = buildConfigs[j].Sources.Where(a=>a.Enabled).ToList();
                tasks.Add(new Tuple<List<UploadConfig.SourceData>, Task<bool>>(activeSources, task));
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                for (int j = 0; j < tasks.Count; j++)
                {
                    Tuple<List<UploadConfig.SourceData>, Task<bool>> tuple = tasks[j];
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

                await Task.Yield();
            }

            return allSuccessful;
        }

        private async Task<bool> GetSources(UploadConfig uploadConfig, UploadTaskReport report, StringFormatter.Context ctx)
        {
            UploadTaskReport.StepResult[] results = report.NewReports(Type, uploadConfig.Sources.Count);
            for (var i = 0; i < uploadConfig.Sources.Count; i++)
            {
                UploadConfig.SourceData sourceData = uploadConfig.Sources[i];
                UploadTaskReport.StepResult result = results[i];
                if (!sourceData.Enabled)
                {
                    result.AddLog("Source skipped - not enabled");
                    continue;
                }

                try
                {
                    bool success = await sourceData.Source.GetSource(uploadConfig, result, ctx);
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
                finally
                {
                    result.SetPercentComplete(1f);
                }
            }

            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            List<UploadConfig> buildConfigs = uploadTask.UploadConfigs;
            for (var i = 0; i < buildConfigs.Count; i++)
            {
                var config = buildConfigs[i];
                if (!config.Enabled)
                {
                    continue;
                }

                UploadTaskReport.StepResult[] results = report.NewReports(Type, config.Sources.Count);
                for (var j = 0; j < config.Sources.Count; j++)
                {
                    if (!config.Sources[j].Enabled)
                    {
                        continue;
                    }

                    AUploadSource source = config.Sources[j].Source;
                    string path = source.SourceFilePath();
                    try{
                        ReportFilesAtPath(path, $"[{source.DisplayName}] FinalSource: {path}", results[j]);
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