using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Execute actions before we begin the upload progress
    /// Each action is executed in order
    /// </summary>
    public class UploadTaskStep_PreUploadActions : AUploadTask_Step
    {
        public UploadTaskStep_PreUploadActions(Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.PreUploadActions;
        public override bool RequiresEverythingBeforeToSucceed => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            UploadTaskReport.StepResult actionResult = report.NewReport(StepType.PreUploadActions);
            int preActionID = ProgressUtils.Start("Pre Upload Actions", "Executing Pre Upload Actions...");
            
            List<UploadConfig.UploadActionData> PreUploadActions = uploadTask.PreUploadActions;
            for (var i = 0; i < PreUploadActions.Count; i++)
            {
                UploadConfig.UploadActionData actionData = PreUploadActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    actionResult.AddLog($"Skipping pre upload action {i+1} because it's null");
                    continue;
                }

                UploadConfig.UploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.UploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfSuccessful && !report.Successful) ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfFailed && report.Successful))
                {
                    actionResult.AddLog($"Skipping pre upload action {i+1} because it doesn't match the current status. Status: {status}. Successful: {report.Successful}");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(preActionID, 0, $"Executing action " + (i+1) + "/" + PreUploadActions.Count);
                    
                actionResult.AddLog($"Executing pre upload action: {i+1}");

                bool prepared = await actionData.UploadAction.Prepare(actionResult);
                if (!prepared)
                {
                    actionResult.AddError($"Failed to prepare pre upload action: {actionData.UploadAction.GetType().Name}");
                    continue;
                }

                try
                {
                    await actionData.UploadAction.Execute(actionResult);
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
            
            ProgressUtils.Remove(preActionID);
            actionResult.SetPercentComplete(1f);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}