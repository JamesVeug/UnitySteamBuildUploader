using System.Collections;
using System.Collections.Generic;

namespace Wireframe
{
    public class NoUploadDestination : ASteamBuildDestination
    {
        public NoUploadDestination(SteamBuildWindow window) : base(window)
        {

        }

        public override void OnGUIExpanded()
        {

        }

        public override void OnGUICollapsed()
        {

        }

        public override IEnumerator Upload(string filePath, string buildDescription)
        {
            m_uploadInProgress = true;
            m_uploadProgress = 1;
            yield return true;
        }

        public override string ProgressTitle()
        {
            return "Uploading nowhere";
        }

        public override bool IsSetup()
        {
            return true;
        }

        public override bool WasUploadSuccessful()
        {
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