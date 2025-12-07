using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class UploadTask
    {
        [Obsolete("Use the constructor with pre and post upload actions", true)]
        public UploadTask(string name, List<UploadConfig> uploadConfigs,
            List<UploadConfig.UploadActionData> postUploadActions) : this()
        {
            this.uploadName = name;
            this.uploadConfigs = uploadConfigs;
            this.postUploadActions = postUploadActions ?? new List<UploadConfig.UploadActionData>();
        }

        [Obsolete("Use the constructor with pre and post upload actions", true)]
        public UploadTask(List<UploadConfig> uploadConfigs, List<UploadConfig.UploadActionData> postUploadActions) 
            : this("No Name Specified", uploadConfigs, postUploadActions)
        {
            
        }
    }
}