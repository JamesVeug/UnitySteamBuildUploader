namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class NintendoService : AService
    {
        public override string ServiceName => "Nintendo";
        public override string[] SearchKeywords => new string[]{"nintendo", "switch", "ndc", "nintendo dev center", "console", "game distribution", "game upload"};

        public NintendoService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!NintendoSDK.Enabled)
            {
                reason = "Nintendo SDK is not enabled in Preferences";
                return false;
            }

            if (!NintendoSDK.Instance.IsInitialized)
            {
                reason = "Nintendo SDK is not initialized";
                return false;
            }

            if (string.IsNullOrEmpty(NintendoSDK.UserName))
            {
                reason = "Nintendo Developer username not set in Preferences";
                return false;
            }

            reason = "";
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
