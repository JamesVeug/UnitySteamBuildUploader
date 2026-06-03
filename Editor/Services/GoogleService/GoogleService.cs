namespace Wireframe
{
    internal partial class GoogleService : AService
    {
        public override string ServiceName => "Google";
        public override string[] SearchKeywords => new string[]{"Google", "Drive", "Chat", "Messaging", "Cloud"};

        public GoogleService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Google.Enabled)
            {
                reason = "Google is not enabled in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            GoogleConfig config = GoogleUIUtils.GetConfig(false);
            if (config == null)
            {
                return false;
            }

            return config.apps.Count > 0
                   || config.driveFolders.Count > 0
                   || config.chatSpaces.Count > 0
                   || config.playApps.Count > 0;
        }
    }
}
