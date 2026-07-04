using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Runs a single <see cref="AUploadAction"/> (Prepare then Execute).
    /// </summary>
    public class ActionNodeV2 : AUploadNodeV2
    {
        public AUploadAction Action { get; }

        private readonly string m_label;

        public ActionNodeV2(AUploadAction action, string label)
        {
            Action = action;
            m_label = label;
        }

        public override async Task<bool> Run(UploadTaskV2 task, UploadTaskReport report, CancellationTokenSource token)
        {
            UploadTaskReport.StepResult result = report.NewReport(AUploadTask_Step.StepType.PostUploadActions);
            result.AddLog(m_label);

            try
            {
                bool prepared = await Action.Prepare(result);
                if (!prepared)
                {
                    return false;
                }

                return await Action.Execute(result);
            }
            catch (Exception e)
            {
                result.AddException(e);
                result.SetFailed("Action failed - " + e.Message);
                return false;
            }
            finally
            {
                result.SetPercentComplete(1f);
            }
        }
    }
}
