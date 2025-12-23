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
        public UploadTaskStep_PostUploadActions(Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.PostUploadActions;
        public override bool RequiresEverythingBeforeToSucceed => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            List<UploadConfig.UploadActionData> postUploadActions = uploadTask.PostUploadActions;
            UploadTaskReport.StepResult[] results = report.NewReports(StepType.PostUploadActions, postUploadActions.Count);
            m_stateResults.Add(new StateResult()
            {
                uploadConfig = null,
                reports = results,
                labelGetter = (index) => "Post Upload Action: " + (index + 1)
            });
            
            int postActionID = ProgressUtils.Start("Post Upload Actions", "Executing Post Upload Actions...");
            
            for (int i = 0; i < postUploadActions.Count; i++)
            {
                UploadTaskReport.StepResult result = results[i];
                UploadConfig.UploadActionData actionData = postUploadActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    result.SetSkipped($"Skipping post upload action {i+1} because it's null");
                    continue;
                }

                UploadConfig.UploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.UploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfSuccessful && !report.Successful) ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfFailed && report.Successful))
                {
                    result.SetSkipped($"Skipping post upload action {i+1} because it doesn't match the current status. Status: {status}. Successful: {report.Successful}");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(postActionID, 0, $"Executing action " + (i+1) + "/" + postUploadActions.Count);
                    
                result.AddLog($"Executing post upload action: {i+1}");

                bool prepared = await actionData.UploadAction.Prepare(result);
                if (!prepared)
                {
                    result.AddError($"Failed to prepare post upload action: {actionData.UploadAction.GetType().Name}");
                    result.SetPercentComplete(1f);
                    continue;
                }

                try
                {
                    await actionData.UploadAction.Execute(result);
                }
                catch (Exception e)
                {
                    result.AddException(e);
                }
                finally
                {
                    result.SetPercentComplete(1f);
                }
            }
            
            ProgressUtils.Remove(postActionID);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}