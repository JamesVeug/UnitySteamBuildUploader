using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class SteamworksService : AService
    {
        public override string ServiceName => "Steamworks";
        public override string[] SearchKeywords => new string[]{"steam", "steamworks", "steam works", "works", "game distribution", "game upload"};

        public SteamworksService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!SteamSDK.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            
            if (!SteamSDK.Instance.IsInitialized)
            {
                reason = PreferencesLink("Steam SDK is not initialized", "Steam SDK has not been setup. Either something isn't set correctly or is failing to setup correctly.");
                return false;
            }

            if (string.IsNullOrEmpty(SteamSDK.UserName))
            {
                reason = PreferencesLink("Steam Username not set", "No Username has been defined in order to begin using Steam");
                return false;
            }

            // GetDisplayStatus rather than CachedStatus so a machine with credentials already saved is not
            // blocked just because nobody has pressed the button to verify them yet.
            AuthStatus status = SteamSDK.GetDisplayStatus();
            if (status == AuthStatus.RequiresLogin)
            {
                reason = PreferencesLink("Steam login required",
                    "'" + SteamSDK.UserName + "' is not authorized on this machine. Press the login button to authorize it.", this);
                return false;
            }

            if (status == AuthStatus.Unknown)
            {
                reason = PreferencesLink("Steam login not checked",
                    "No saved login was found for '" + SteamSDK.UserName + "' on this machine. Press the button to check.", this);
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            SteamAppData data = SteamUIUtils.GetSteamBuildData(false);
            if (data == null)
            {
                return false;
            }

            return data.Configs.Count > 0;
        }
    }
}