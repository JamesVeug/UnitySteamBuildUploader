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
        public override bool FireActions => false;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report,
            CancellationTokenSource token)
        {
            int postUpload = ProgressUtils.Start("Post Upload Actions", "Executing Post Upload Actions...");
            
            await uploadTask.ExecuteActions(uploadTask.IsSuccessful, UploadConfig.UploadActionData.UploadTrigger.OnTaskFinished);
            
            ProgressUtils.Remove(postUpload);
            return true;
        }

        public override Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report, bool allStepsSuccessful)
        {
            return Task.FromResult(true);
        }
    }
}