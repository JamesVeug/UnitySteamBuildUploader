using UnityEngine;

namespace Wireframe
{
    [Experimental]
    internal partial class AppleService : AService
    {
        public static AppleService Instance => InternalUtils.GetService<AppleService>();
        
        public override string ServiceName => "Apple";
        public GUIContent NotRunningMacUI => new GUIContent("You are not running MacOS", "Apple uploads via xcrun altool which requires macOS. You are running " + System.Environment.OSVersion.Platform);

        public override string[] SearchKeywords => new string[]
        {
            "apple", "iOS", "tvOS", "macOS", "visionOS", "testFlight", "app store", "app store connect", "ipa"
        };

        public AppleService()
        {
            // Required for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Apple.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            if (!Apple.IsRunningOnMac)
            {
                reason = NotRunningMacUI;
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            AppleConfig config = AppleUIUtils.GetConfig(false);
            if (config == null)
            {
                return false;
            }

            return config.apps.Count > 0 && config.apiKeys.Count > 0;
        }
    }
}
