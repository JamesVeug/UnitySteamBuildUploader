using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Execute actions after the upload has completed, regardless of success or failure
    /// Each action is executed in order
    /// </summary>
    public class UploadTaskStep_PostUploadActions : AUploadTask_Step
    {
        public UploadTaskStep_PostUploadActions(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.PostUploadActions;
        public override bool RequiresEverythingBeforeToSucceed => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            UploadTaskReport.StepResult actionResult = report.NewReport(StepType.PostUploadActions);
            int postActionID = ProgressUtils.Start("Post Upload Actions", "Executing Post Upload Actions...");
            
            List<UploadConfig.PostUploadActionData> postUploadActions = uploadTask.PostUploadActions;
            for (var i = 0; i < postUploadActions.Count; i++)
            {
                UploadConfig.PostUploadActionData actionData = postUploadActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    actionResult.AddLog($"Skipping post upload action {i+1} because it's null");
                    continue;
                }

                UploadConfig.PostUploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.IfSuccessful && !report.Successful) ||
                    (status == UploadConfig.PostUploadActionData.UploadCompleteStatus.IfFailed && report.Successful))
                {
                    actionResult.AddLog($"Skipping post upload action {i+1} because it doesn't match the current status. Status: {status}. Successful: {report.Successful}");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(postActionID, 0, $"Executing action " + (i+1) + "/" + postUploadActions.Count);
                    
                actionResult.AddLog($"Executing post upload action: {i+1}");

                bool prepared = await actionData.UploadAction.Prepare(actionResult);
                if (!prepared)
                {
                    actionResult.AddError($"Failed to prepare post upload action: {actionData.UploadAction.GetType().Name}");
                    continue;
                }

                try
                {
                    await actionData.UploadAction.Execute(actionResult, m_context);
                }
                catch (Exception e)
                {
                    actionResult.AddException(e);
                }
                finally
                {
                    actionResult.SetPercentComplete(1f);
                }
            }
            
            ProgressUtils.Remove(postActionID);
            actionResult.SetPercentComplete(1f);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}