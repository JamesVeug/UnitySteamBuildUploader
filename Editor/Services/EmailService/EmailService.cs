using UnityEngine;

namespace Wireframe
{
    [Experimental]
    internal partial class EmailService : AService
    {
        public static EmailService Instance => InternalUtils.GetService<EmailService>();

        public override string ServiceName => "Email";
        public override string[] SearchKeywords => new string[]{"email", "mail", "smtp", "send mail", "messaging"};

        public EmailService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Email.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            EmailConfig config = EmailUIUtils.GetConfig(false);
            if (config == null || config.accounts == null || config.accounts.Count == 0)
            {
                reason = ProjectSettingsLink("No Email accounts", "Email accounts are used to send emails to people");
                return false;
            }

            reason = null;
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
