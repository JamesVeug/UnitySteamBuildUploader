using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class EMailService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new EMailService_Preferences_Provider("Preferences/Build Uploader/Services/EMail", SettingsScope.User)
                {
                    label = "EMail",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is EMailService).SearchKeywords
                };
            return provider;
        }

        private EMailService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is EMailService)?.PreferencesGUI();
        }
    }
}
