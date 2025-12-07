using System.Collections.Generic;

namespace Wireframe
{
    internal partial class WindowUploadTab : IContextContainer
    {
        List<UploadConfig> IContextContainer.UploadConfigs => m_currentUploadProfile.UploadConfigs;
        List<UploadConfig.UploadActionData> IContextContainer.PreUploadActions => m_currentUploadProfile.PreUploadActions;
        List<UploadConfig.UploadActionData> IContextContainer.PostUploadActions => m_currentUploadProfile.PostUploadActions;
    }
}