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
        public override bool FireActions => false;

        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            int preActionID = ProgressUtils.Start("Pre Upload Actions", "Executing Pre Upload Actions...");

            await uploadTask.ExecuteActions(uploadTask.IsSuccessful, UploadConfig.UploadActionData.UploadTrigger.OnTaskStarted);
            
            ProgressUtils.Remove(preActionID);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report, bool allStepsSuccessful)
        {
            return Task.FromResult(true);
        }
    }
}