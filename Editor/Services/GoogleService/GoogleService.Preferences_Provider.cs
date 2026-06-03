using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class GoogleService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new GoogleService_Preferences_Provider("Preferences/Build Uploader/Services/Google", SettingsScope.User)
                {
                    label = "Google",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is GoogleService).SearchKeywords
                };
            return provider;
        }

        private GoogleService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is GoogleService)?.PreferencesGUI();
        }
    }
}
