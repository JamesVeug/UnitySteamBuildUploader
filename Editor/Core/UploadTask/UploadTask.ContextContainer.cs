using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wireframe
{
    public partial class UploadTask : IContextContainer
    {
        List<UploadConfig> IContextContainer.UploadConfigs => m_uploadConfigs;
        List<UploadConfig.UploadActionData> IContextContainer.Actions => m_actions;

        public string UploadStatus
        {
            get
            {
                // Uploading 'Debug Build'
                // Status: In Progress...
                // - Step: Upload
                // - Progress 50%
                string uploadNumber = m_context.FormatString(Wireframe.Context.UPLOAD_NUMBER_KEY);
                
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Upload #{uploadNumber} {m_uploadName}");
                if (!HasStarted)
                {
                    builder.AppendLine("- Status: Not started yet");
                    return builder.ToString();
                }
                
                if (IsComplete || CurrentStepType >= AUploadTask_Step.StepType.PostUploadActions)
                {
                    string icon = IsSuccessful ? "✅" : "❌";
                    string state = IsSuccessful ? "Successful" : "Failed!";
                    builder.AppendLine($"- Status: {icon} {state}");
                    if (!IsSuccessful)
                    {
                        foreach (var failReason in m_report.GetFailReasons())
                        {
                            builder.AppendLine($"- {failReason.Key}: {failReason.FailReason}");
                        }
                    }
                    UploadTaskStep_Upload step = GetStep<UploadTaskStep_Upload>();
                    builder.AppendLine(step.GetStateSummary());
                }
                else
                {
                    builder.AppendLine($"- Status: ⌛ In Progress... {Mathf.CeilToInt(PercentComplete * 100)}%");
                    builder.AppendLine($"- Step: {CurrentStepType}");
                    
                    // Steam: Making build...
                    // Itchio: Uploading build...
                    // LocalPath: Waiting to progress to the next step... 
                    // Epic: Completed! 🚀
                    // Github: Modifying build...
                    builder.AppendLine(CurrentStep.GetStateSummary());
                }
                
                
                return builder.ToString();
            }
        }
    }
}
