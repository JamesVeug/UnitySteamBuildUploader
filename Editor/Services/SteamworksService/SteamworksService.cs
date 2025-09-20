﻿namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class SteamworksService : AService
    {
        public SteamworksService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!SteamSDK.Enabled)
            {
                reason = "Steam SDK is not enabled in Preferences";
                return false;
            }

            
            if (!SteamSDK.Instance.IsInitialized)
            {
                reason = "Steam SDK is not initialized";
                return false;
            }

            if (string.IsNullOrEmpty(SteamSDK.UserName))
            {
                reason = "Steam Username not set in Preferences";
                return false;
            }

            reason = "";
            return true;
        }
    }
}