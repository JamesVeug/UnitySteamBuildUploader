using System.Collections.Generic;
using System.Text;

namespace Wireframe
{
    public partial class UploadTask : IContextContainer
    {
        List<UploadConfig> IContextContainer.UploadConfigs => uploadConfigs;
        List<UploadConfig.UploadActionData> IContextContainer.PreUploadActions => preUploadActions;
        List<UploadConfig.UploadActionData> IContextContainer.PostUploadActions => postUploadActions;

        public string UploadStatus
        {
            get
            {
                // Uploading 'Debug Build'
                // Status: In Progress...
                // - Step: Upload
                // - Progress 50%
                string uploadNumber = context.FormatString(Wireframe.Context.UPLOAD_NUMBER_KEY);
                
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Upload #{uploadNumber} {uploadName}");
                if (!HasStarted)
                {
                    builder.AppendLine("Status: Not started yet");
                    return builder.ToString();
                }
                
                if (IsComplete)
                {
                    string icon = IsSuccessful ? "✅" : "❌";
                    string state = IsSuccessful ? "Successful" : "Failed!";
                    builder.AppendLine($"Status: {icon} {state}");
                    if (!IsSuccessful)
                    {
                        foreach (var failReason in report.GetFailReasons())
                        {
                            builder.AppendLine($"- {failReason.Key}: {failReason.FailReason}");
                        }
                    }
                    
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine($"Status: In Progress...");
                }
                
                builder.AppendLine($"- Step: {CurrentStepType}");
                builder.AppendLine($"- Progress: {PercentComplete * 100}%");
                
                // Steam: Making build...
                // Itchio: Uploading build...
                // LocalPath: Waiting to progress to the next step... 
                // Epic: Completed! 🚀
                // Github: Modifying build...
                builder.AppendLine(CurrentStep.GetStateSummary());
                
                return builder.ToString();
            }
        }
    }
}