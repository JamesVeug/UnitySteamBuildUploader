using System.Collections.Generic;

namespace Wireframe
{
    public partial class UploadTask : IContextContainer
    {
        List<UploadConfig> IContextContainer.UploadConfigs => uploadConfigs;
        List<UploadConfig.UploadActionData> IContextContainer.PreUploadActions => preUploadActions;
        List<UploadConfig.UploadActionData> IContextContainer.PostUploadActions => postUploadActions;
    }
}