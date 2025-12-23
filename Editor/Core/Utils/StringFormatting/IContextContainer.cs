using System.Collections.Generic;

namespace Wireframe
{
    public interface IContextContainer
    {
        public List<UploadConfig> UploadConfigs { get; }
        public List<UploadConfig.UploadActionData> PreUploadActions { get; }
        public List<UploadConfig.UploadActionData> PostUploadActions { get; }
        string UploadName { get; }
        string UploadDescription { get; }
        string UploadStatus { get; }
    }
}