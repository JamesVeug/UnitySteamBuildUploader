using System.Collections.Generic;

namespace Wireframe
{
    public interface IContextContainer
    {
        List<UploadConfig> UploadConfigs { get; }
        List<UploadConfig.UploadActionData> Actions { get; }
        string UploadName { get; }
        string UploadDescription { get; }
        string UploadStatus { get; }
    }
}