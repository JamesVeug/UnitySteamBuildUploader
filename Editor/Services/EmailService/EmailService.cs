namespace Wireframe
{
    internal partial class EmailService : AService
    {
        public override string ServiceName => "Email";
        public override string[] SearchKeywords => new string[]{"email", "mail", "smtp", "send mail", "messaging"};

        public EmailService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Email.Enabled)
            {
                reason = "Email is not enabled in Preferences";
                return false;
            }

            EmailConfig config = EmailUIUtils.GetConfig(false);
            if (config == null || config.accounts == null || config.accounts.Count == 0)
            {
                reason = "Email has no accounts configured. Add one in Project Settings -> Build Uploader -> Services -> Email.";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            EmailConfig config = EmailUIUtils.GetConfig(false);
            if (config == null)
            {
                return false;
            }

            return config.accounts != null && config.accounts.Count > 0;
        }
    }
}
