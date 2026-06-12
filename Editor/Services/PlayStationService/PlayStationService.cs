using UnityEngine;

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

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!PlayStationSDK.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            if (!PlayStationSDK.Instance.IsInitialized)
            {
                reason = PreferencesLink("PlayStation SDK is not initialized", "PlayStation SDK has not been setup. Either something isn't set correctly or is failing to setup correctly.");
                return false;
            }

            if (string.IsNullOrEmpty(PlayStationSDK.UserName))
            {
                reason = PreferencesLink("PlayStation Developer username not set", "Username is required to use to upload to Play Statio");
                return false;
            }

            reason = null;
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
