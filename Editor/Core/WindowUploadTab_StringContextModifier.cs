using System.Collections.Generic;
using System.Text;

namespace Wireframe
{
    internal partial class WindowUploadTab : IContextContainer
    {
        List<UploadConfig> IContextContainer.UploadConfigs => m_currentUploadProfile.UploadConfigs;
        List<UploadConfig.UploadActionData> IContextContainer.Actions => m_currentUploadProfile.Actions;

        public string UploadDescription => m_buildDescription;
        
        public string UploadName
        {
            get
            {
                return m_currentUploadProfile == null ? "No Profile selected" : m_currentUploadProfile.ProfileName;
            }
        }

        public string UploadStatus
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Upload #{uploadNumber} {taskProfileName}");
                builder.AppendLine("Status: Not started yet");
                return builder.ToString();
            }
        }
    }
}