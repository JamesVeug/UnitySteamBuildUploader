namespace Wireframe
{
    [Experimental]
    internal partial class PlayStationService : AService
    {
        public override string ServiceName => "PlayStation";
        public override string[] SearchKeywords => new string[]{"playstation", "ps4", "ps5", "sony", "prospero", "orbis", "partners", "console", "game distribution", "game upload"};

        public PlayStationService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!PlayStationSDK.Enabled)
            {
                reason = "PlayStation SDK is not enabled in Preferences";
                return false;
            }

            if (!PlayStationSDK.Instance.IsInitialized)
            {
                reason = "PlayStation SDK is not initialized";
                return false;
            }

            if (string.IsNullOrEmpty(PlayStationSDK.UserName))
            {
                reason = "PlayStation Developer username not set in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            PlayStationAppData data = PlayStationUIUtils.GetPlayStationBuildData(false);
            if (data == null)
            {
                return false;
            }

            return data.Configs.Count > 0;
        }
    }
}
