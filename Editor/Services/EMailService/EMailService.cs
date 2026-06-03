namespace Wireframe
{
    internal partial class EMailService : AService
    {
        public override string ServiceName => "EMail";
        public override string[] SearchKeywords => new string[]{"EMail", "Email", "Mail", "SMTP", "Send Mail", "Messaging"};

        public EMailService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!EMail.Enabled)
            {
                reason = "EMail is not enabled in Preferences";
                return false;
            }

            EMailConfig config = EMailUIUtils.GetConfig(false);
            if (config == null || config.accounts == null || config.accounts.Count == 0)
            {
                reason = "EMail has no accounts configured. Add one in Project Settings -> Build Uploader -> Services -> EMail.";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            EMailConfig config = EMailUIUtils.GetConfig(false);
            if (config == null)
            {
                return false;
            }

            return config.accounts != null && config.accounts.Count > 0;
        }
    }
}
