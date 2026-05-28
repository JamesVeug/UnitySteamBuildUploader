using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class XboxService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new XboxService_Preferences_Provider(
                "Preferences/Build Uploader/Services/Xbox", SettingsScope.User)
            {
                label    = "Xbox",
                keywords = InternalUtils.AllServices()
                    .FirstOrDefault(a => a is XboxService)?.SearchKeywords
            };
            return provider;
        }

        private XboxService_Preferences_Provider(
            string path,
            SettingsScope scopes,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is XboxService)?.PreferencesGUI();
        }
    }
}
