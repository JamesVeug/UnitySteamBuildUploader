using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class AppleService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new AppleService_Preferences_Provider("Preferences/Build Uploader/Services/Apple", SettingsScope.User)
                {
                    label = "Apple",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is AppleService).SearchKeywords
                };
            return provider;
        }

        private AppleService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is AppleService)?.PreferencesGUI();
        }
    }
}
