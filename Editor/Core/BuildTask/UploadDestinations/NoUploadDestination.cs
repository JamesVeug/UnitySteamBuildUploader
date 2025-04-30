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
    [Wiki("NowhereDestination", "destinations", "Does not move the files anywhere. Useful for testing sources and modifiers.")]
    [BuildDestination("Nowhere")]
    public class NoUploadDestination : ABuildDestination
    {
        public NoUploadDestination() : base()
        {
            // Required for reflection
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
        {

        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {

        }

        public override Task<bool> Upload(BuildTaskReport.StepResult result)
        {
            m_uploadProgress = 1;
            return Task.FromResult(true);
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