using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// A no destination end point when uploading a build
    /// Good for testing if your builds can be retrieved from the sources without worrying if they are uploading or not
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    internal class NoUploadDestination : ABuildDestination
    {
        public override string DisplayName => "Nowhere";

        public NoUploadDestination(BuildUploaderWindow window) : base(window)
        {
        }
        
        public override void OnGUIExpanded(ref bool isDirty)
        {

        }

        public override void OnGUICollapsed(ref bool isDirty)
        {

        }

        public override Task<UploadResult> Upload(string filePath, string buildDescription)
        {
            m_uploadInProgress = true;
            m_uploadProgress = 1;
            return Task.FromResult(UploadResult.Success());
        }

        public override string ProgressTitle()
        {
            return "Uploading nowhere";
        }

        public override bool IsSetup(out string reason)
        {
            reason = "";
            return true;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {

        }
    }
}