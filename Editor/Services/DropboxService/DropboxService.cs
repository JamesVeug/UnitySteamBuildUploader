using UnityEngine;

namespace Wireframe
{
    [Experimental]
    internal partial class DropboxService : AService
    {
        public static DropboxService Instance => InternalUtils.GetService<DropboxService>();

        public override string ServiceName => "Dropbox";
        public override string[] SearchKeywords => new string[] { "dropbox", "upload", "cloud", "storage" };

        public DropboxService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Dropbox.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            DropboxConfig config = DropboxUIUtils.GetConfig(false);
            if (config == null)
            {
                return false;
            }

            return config.apps.Count > 0 || config.folders.Count > 0;
        }
    }
}
