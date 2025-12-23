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
            int preActionID = ProgressUtils.Start("Pre Upload Actions", "Executing Pre Upload Actions...");
            List<UploadConfig.UploadActionData> preUploadActions = uploadTask.PreUploadActions;
            
            UploadTaskReport.StepResult[] results = report.NewReports(StepType.PreUploadActions, preUploadActions.Count);
            m_stateResults.Add(new StateResult()
            {
                uploadConfig = null,
                reports =  results,
                labelGetter = (index) => "Pre Upload Action: " + (index + 1),
            });
            
            for (var i = 0; i < preUploadActions.Count; i++)
            {
                UploadTaskReport.StepResult result  = results[i];
                UploadConfig.UploadActionData actionData = preUploadActions[i];
                if (actionData == null || actionData.UploadAction == null)
                {
                    result.SetSkipped($"Skipping pre upload action {i+1} because it's null");
                    continue;
                }

                UploadConfig.UploadActionData.UploadCompleteStatus status = actionData.WhenToExecute;
                if (status == UploadConfig.UploadActionData.UploadCompleteStatus.Never ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfSuccessful && !report.Successful) ||
                    (status == UploadConfig.UploadActionData.UploadCompleteStatus.IfFailed && report.Successful))
                {
                    result.SetSkipped($"Skipping pre upload action {i+1} because it doesn't match the current status. Status: {status}. Successful: {report.Successful}");
                    continue;
                }

                await Task.Yield();
                ProgressUtils.Report(preActionID, 0, $"Executing action " + (i+1) + "/" + preUploadActions.Count);
                    
                result.AddLog($"Executing pre upload action: {i+1}");

                bool prepared = await actionData.UploadAction.Prepare(result);
                if (!prepared)
                {
                    result.AddError($"Failed to prepare pre upload action: {actionData.UploadAction.GetType().Name}");
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
            
            ProgressUtils.Remove(preActionID);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            return Task.FromResult(true);
        }
    }
}