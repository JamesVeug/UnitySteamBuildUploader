namespace Wireframe
{
    public partial class AppleUploadDestination
    {
        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.APPLE_APP_NAME_KEY, GetAppName);
            context.AddCommand(Context.APPLE_BUNDLE_ID_KEY, GetBundleId);
            context.AddCommand(Context.APPLE_BUILD_ID_KEY, GetBuildId);
            context.AddCommand(Context.APPLE_BUILD_VERSION_KEY, GetBuildVersion);
            context.AddCommand(Context.APPLE_BUILD_NUMBER_KEY, GetBuildNumber);
            return context;
        }

        private string GetAppName()
        {
            return m_app != null ? m_app.DisplayName : "Unspecified App";
        }

        private string GetBundleId()
        {
            return m_app != null ? m_app.BundleID : "Unspecified Bundle ID";
        }

        private string GetBuildId()
        {
            return string.IsNullOrEmpty(m_lastBuildId) ? "Unspecified Build ID" : m_lastBuildId;
        }

        private string GetBuildVersion()
        {
            return string.IsNullOrEmpty(m_lastBuildVersion) ? "Unspecified Build Version" : m_lastBuildVersion;
        }

        private string GetBuildNumber()
        {
            return string.IsNullOrEmpty(m_lastBuildNumber) ? "Unspecified Build Number" : m_lastBuildNumber;
        }
    }
}
