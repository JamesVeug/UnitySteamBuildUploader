using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Runs a single <see cref="AUploadSource"/> (Prepare then GetSource) and exposes its resulting
    /// content path via <see cref="ResolvedContentPath"/> once it has run, for downstream
    /// <see cref="DestinationNodeV2"/> nodes to consume.
    /// </summary>
    public class SourceNodeV2 : AUploadNodeV2
    {
        public AUploadSource Source { get; }
        public string ResolvedContentPath { get; private set; } = "";

        private readonly string m_label;

        public SourceNodeV2(AUploadSource source, string label)
        {
            Source = source;
            m_label = label;
        }

        public override async Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.GetSources);
            result.AddLog(m_label);

            try
            {
                string contentFolder = task.AllocateSourceFolder();
                bool prepared = await Source.Prepare(contentFolder, result, token);
                if (!prepared)
                {
                    return false;
                }

                bool gotSource = await Source.GetSource(true, task.DummyConfig, result, token);
                if (!gotSource)
                {
                    return false;
                }

                ResolvedContentPath = Source.SourceFilePath();
                return true;
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
    }
}
