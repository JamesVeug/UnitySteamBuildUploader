namespace Wireframe
{
    internal partial class SlackService : AService
    {
        public override string ServiceName => "Slack";
        public override string[] SearchKeywords => new string[]{"Slack", "Messaging", "Chat"};

        public SlackService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Slack.Enabled)
            {
                reason = "Slack is not enabled in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            SlackConfig SlackConfig = SlackUIUtils.GetConfig(false);
            if (SlackConfig == null)
            {
                return false;
            }

            return SlackConfig.servers.Count > 0;
        }
    }
}