using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Base class for a step in the upload task process
    /// Each step is executed in order and can specify what happens if the step fails
    /// Steps can be things like validation, preparing sources, uploading, post upload actions, etc
    /// </summary>
    public abstract class AUploadTask_Step
    {
        protected class StateResult
        {
            public UploadConfig uploadConfig;
            public Func<int, string> labelGetter;
            public UploadTaskReport.StepResult[] reports;
        }

        public enum StepType
        {
            Validation,
            PreUploadActions,
            PrepareSources,
            GetSources,
            CacheSources,
            ModifyCacheSources,
            PrepareDestinations,
            Upload,
            PostUploadActions,
            Cleanup
        }

        public enum StepProcess
        {
            Pre,
            Intra,
            Post
        }
        
        public abstract StepType Type { get; }
        public virtual bool RequiresEverythingBeforeToSucceed => true;
        public abstract Task<bool> Run(UploadTask uploadTask, UploadTaskReport report, CancellationTokenSource token);
        public abstract Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report);
        
        protected List<StateResult> m_stateResults = new List<StateResult>(); 
        protected bool m_completed;
        protected bool m_successful;
        protected readonly Context m_context;

        public AUploadTask_Step(Context ctx)
        {
            m_context = ctx;
        }
        
        protected void ReportCachedFiles(UploadTask uploadTask, UploadTaskReport report)
        {
            List<UploadConfig> buildConfigs = uploadTask.UploadConfigs;
            UploadTaskReport.StepResult[] results = report.NewReports(Type, buildConfigs.Count);
            for (var i = 0; i < buildConfigs.Count; i++)
            {
                var config = buildConfigs[i];
                var result = results[i];
                if (!config.Enabled)
                {
                    continue;
                }
                
                string cachePath = uploadTask.CachedLocations[i];
                if (string.IsNullOrEmpty(cachePath))
                {
                    result.AddLog($"Config {i+1} has cached files. Something went wrong.");
                    return;
                }
                
                ReportFilesAtPath(cachePath, $"Config {i+1} cached files:", result);
            }
        }

        protected void ReportFilesAtPath(string cachePath, string prefix, UploadTaskReport.StepResult result)
        {
            if (string.IsNullOrEmpty(cachePath))
            {
                return;
            }
            
            List<string> allFiles = Utils.GetSortedFilesAndDirectories(cachePath);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(prefix);
            foreach (string file in allFiles)
            {
                sb.AppendLine("\t-" + file);
            }
            result.AddLog(sb.ToString());
        }

        public virtual string GetStateSummary()
        {
            StringBuilder summary = new StringBuilder();
            foreach (StateResult stateResult in m_stateResults)
            {
                for (var i = 0; i < stateResult.reports.Length; i++)
                {
                    string labelGetter = stateResult.labelGetter(i);
                    summary.Append(labelGetter);
                    summary.Append(": ");
                    
                    UploadTaskReport.StepResult stepResult = stateResult.reports[i];
                    if (stepResult.IsSkipped)
                    {
                        summary.AppendLine("Skipped");
                    }
                    else if (stepResult.PercentComplete >= 1f)
                    {
                        if (stepResult.Successful)
                        {
                            summary.AppendLine("Complete");
                        }
                        else
                        {
                            summary.Append("Failed - ");
                            summary.AppendLine(stepResult.FailReason);
                        }
                    }
                    else if (stepResult.Logs.Count > 0)
                    {
                        summary.AppendLine(stepResult.Logs[stepResult.Logs.Count - 1].Message);
                    }
                    else
                    {
                        summary.AppendLine("Waiting to start...");
                    }
                }
            }
            return summary.ToString();
        }
    }
}