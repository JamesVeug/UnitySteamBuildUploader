namespace Wireframe
{
    internal partial class XboxService : AService
    {
        public override string ServiceName => "Xbox";

        public override string[] SearchKeywords => new string[]
        {
            "xbox", "microsoft", "partner Center", "gdk", "game distribution", "game upload",
            "windows store", "microsoft store"
        };

        public XboxService()
        {
            // Required for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Xbox.Enabled)
            {
                reason = "Xbox is not enabled in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            XboxConfig config = XboxUIUtils.GetConfig(false);
            if (config == null) return false;
            return config.apps != null && config.apps.Count > 0;
        }
    }
}
