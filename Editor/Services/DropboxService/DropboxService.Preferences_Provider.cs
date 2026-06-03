using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class DropboxService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new DropboxService_Preferences_Provider("Preferences/Build Uploader/Services/Dropbox", SettingsScope.User)
                {
                    label = "Dropbox",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is DropboxService).SearchKeywords
                };
            return provider;
        }

        private DropboxService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is DropboxService)?.PreferencesGUI();
        }
    }
}
