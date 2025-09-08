using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract class AUploadTask_Step
    {
        public enum StepType
        {
            Validation,
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
        
        protected readonly StringFormatter.Context m_context;

        public AUploadTask_Step(StringFormatter.Context ctx)
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
    }
}