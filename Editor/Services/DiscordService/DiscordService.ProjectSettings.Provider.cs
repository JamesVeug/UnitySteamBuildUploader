using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class DiscordService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new DiscordService_ProjectSettings_Provider("Project/Build Uploader/Services/Discord", SettingsScope.Project)
                {
                    label = "Discord",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is DiscordService).SearchKeyworks
                };
            return provider;
        }
        
        private DiscordService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is DiscordService)?.ProjectSettingsGUI();
        }
    }
}