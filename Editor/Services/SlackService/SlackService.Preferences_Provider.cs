using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class SlackService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new SlackService_Preferences_Provider("Preferences/Build Uploader/Services/Slack", SettingsScope.User)
                {
                    label = "Slack",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is SlackService).SearchKeyworks
                };
            return provider;
        }
        
        private SlackService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is SlackService)?.PreferencesGUI();
        }
    }
}