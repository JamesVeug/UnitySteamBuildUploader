namespace Wireframe
{
    internal partial class AppleService : AService
    {
        public override string ServiceName => "Apple";

        public override string[] SearchKeywords => new string[]
        {
            "apple", "iOS", "tvOS", "macOS", "visionOS", "testFlight", "app store", "app store connect", "ipa"
        };

        public AppleService()
        {
            // Required for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Apple.Enabled)
            {
                reason = "Apple is not enabled in Preferences";
                return false;
            }

            if (!Apple.IsRunningOnMac)
            {
                reason = "Apple uploads via xcrun altool which requires macOS. You are running " + System.Environment.OSVersion.Platform;
                return false;
            }

            reason = "";
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
