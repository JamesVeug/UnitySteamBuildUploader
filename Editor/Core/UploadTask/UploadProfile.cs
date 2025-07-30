using System.Collections.Generic;

namespace Wireframe
{
    public class UploadProfile
    {
        public string GUID;
        public string ProfileName;
        public List<UploadConfig> UploadConfigs = new List<UploadConfig>();
        public List<UploadConfig.PostUploadActionData> PostUploadActions = new List<UploadConfig.PostUploadActionData>();
    }
}