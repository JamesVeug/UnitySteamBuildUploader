using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Runs a single <see cref="AUploadDestination"/> (Prepare, Upload, PostUpload).
    ///
    /// <see cref="ContentPathProvider"/> is resolved lazily (only when this node actually runs) so it
    /// works regardless of where the upstream source sits in the compiled tree - it just needs to have
    /// already produced its content by the time this destination runs.
    /// </summary>
    public class DestinationNodeV2 : AUploadNodeV2
    {
        public AUploadDestination Destination { get; }
        public Func<string> ContentPathProvider { get; }

        private readonly string m_label;

        public DestinationNodeV2(AUploadDestination destination, Func<string> contentPathProvider, string label)
        {
            Destination = destination;
            ContentPathProvider = contentPathProvider;
            m_label = label;
        }

        public override async Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.Upload);
            result.AddLog(m_label);

            try
            {
                string contentPath = ContentPathProvider?.Invoke() ?? "";
                if (string.IsNullOrEmpty(contentPath))
                {
                    // Fail fast with a clear reason instead of letting Prepare/Upload run with an empty path -
                    // AUploadDestination.Prepare() always succeeds regardless of what it's given, so without this
                    // check the failure instead surfaces later as a cryptic Path.GetFullPath("") ArgumentException
                    // deep inside Upload().
                    result.SetFailed("Content Path resolved to an empty string. Wire this destination's Content Path " +
                                      "to a source that runs before it (e.g. a BuildProfileSourceNode's Output Path), " +
                                      "or to a GetVariableNode/SetVariableNode chain that has actually produced a value by now.");
                    return false;
                }

                bool prepared = await Destination.Prepare(task.GUID, 0, task.NextDestinationIndex(), contentPath, result);
                if (!prepared)
                {
                    return false;
                }

                bool uploaded = await Destination.Upload(result);
                if (!uploaded)
                {
                    return false;
                }

                return await Destination.PostUpload(result);
            }
            catch (Exception e)
            {
                result.AddException(e);
                result.SetFailed("Destination failed - " + e.Message);
                return false;
            }
            finally
            {
                result.SetPercentComplete(1f);
            }
        }
    }
}
