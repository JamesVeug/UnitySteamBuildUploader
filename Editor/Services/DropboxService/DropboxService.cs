namespace Wireframe
{
    [Experimental]
    internal partial class DropboxService : AService
    {
        public override string ServiceName => "Dropbox";
        public override string[] SearchKeywords => new string[] { "dropbox", "upload", "cloud", "storage" };

        public DropboxService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Dropbox.Enabled)
            {
                reason = "Dropbox is not enabled in Preferences";
                return false;
            }

            reason = "";
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
