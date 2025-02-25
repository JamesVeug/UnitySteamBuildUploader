using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class SteamworksService : AService
    {
        private static string steamPasswordConfirmation;
        private static bool steamPasswordAssigned = false;

        public SteamworksService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!SteamSDK.Instance.IsInitialized)
            {
                reason = "Steam SDK is not initialized";
                return false;
            }

            if (string.IsNullOrEmpty(SteamSDK.UserName) ||
                string.IsNullOrEmpty(SteamSDK.UserPassword))
            {
                reason = "Steam SDK credentials are not set";
                return false;
            }

            reason = "";
            return true;
        }
    }
}