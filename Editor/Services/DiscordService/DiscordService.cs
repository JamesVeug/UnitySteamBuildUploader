using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class DiscordService : AService
    {
        public override string ServiceName => "Discord";
        public override string[] SearchKeywords => new string[]{"discord", "chat", "messaging"};
        
        public DiscordService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Discord.Enabled)
            {
                reason = "Discord is not enabled in Preferences";
                return false;
            }

            reason = "";
            return true;
        }
    }
}