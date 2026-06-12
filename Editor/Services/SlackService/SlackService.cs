using UnityEngine;

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
        
        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Slack.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            reason = null;
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