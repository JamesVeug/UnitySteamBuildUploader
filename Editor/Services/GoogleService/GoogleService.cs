using UnityEngine;

namespace Wireframe
{
    [Experimental]
    internal partial class GoogleService : AService
    {
        public override string ServiceName => "Google";
        public override string[] SearchKeywords => new string[]{"google", "drive", "chat", "messaging", "cloud"};

        public GoogleService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Google.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            reason = null;
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
