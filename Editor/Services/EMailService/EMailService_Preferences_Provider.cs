using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class EmailService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new EmailService_Preferences_Provider("Preferences/Build Uploader/Services/Email", SettingsScope.User)
                {
                    label = "Email",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is EmailService).SearchKeywords
                };
            return provider;
        }

        private EmailService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is EmailService)?.PreferencesGUI();
        }
    }
}
