using UnityEngine;

namespace Wireframe
{
    [Experimental]
    internal partial class NintendoService : AService
    {
        public override string ServiceName => "Nintendo";
        public override string[] SearchKeywords => new string[]{"nintendo", "switch", "ndc", "nintendo dev center", "console", "game distribution", "game upload"};

        public NintendoService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!NintendoSDK.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            if (!NintendoSDK.Instance.IsInitialized)
            {
                reason = PreferencesLink("Nintendo SDK is not initialized", "Nintendo SDK has not been setup. Either something isn't set correctly or is failing to setup correctly.");
                return false;
            }

            if (string.IsNullOrEmpty(NintendoSDK.UserName))
            {
                reason = PreferencesLink("Nintendo Developer username not set", "Username is required to upload to Nintendo");
                return false;
            }
            
            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            NintendoAppData data = NintendoUIUtils.GetNintendoBuildData(false);
            if (data == null)
            {
                return false;
            }

            return data.Configs.Count > 0;
        }
    }
}
