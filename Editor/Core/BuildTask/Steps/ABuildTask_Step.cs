using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract class ABuildTask_Step
    {
        public enum StepType
        {
            GetSources,
            CacheSources,
            ModifyCacheSources,
            PrepareDestinations,
            Upload,
            Cleanup
        }

        public enum StepProcess
        {
            Pre,
            Intra,
            Post
        }
        
        public abstract StepType Type { get; }
        public abstract Task<bool> Run(BuildTask buildTask, BuildTaskReport report);
        public abstract Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report);
        
        protected void ReportCachedFiles(BuildTask buildTask, BuildTaskReport report)
        {
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;
            BuildTaskReport.StepResult[] results = report.NewReports(Type, buildConfigs.Count);
            for (var i = 0; i < buildConfigs.Count; i++)
            {
                var config = buildConfigs[i];
                var result = results[i];
                if (!config.Enabled)
                {
                    continue;
                }
                
                string cachePath = buildTask.CachedLocations[i];
                if (string.IsNullOrEmpty(cachePath))
                {
                    result.AddLog($"Config {i+1} has cached files. Something went wrong.");
                    return;
                }
                
                ReportFilesAtPath(cachePath, $"Config {i+1} cached files:", result);
            }
        }

        protected void ReportFilesAtPath(string cachePath, string prefix, BuildTaskReport.StepResult result)
        {
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