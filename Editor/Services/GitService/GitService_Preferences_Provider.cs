using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class GitService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new GitService_Preferences_Provider("Preferences/Build Uploader/Services/Git", SettingsScope.User)
                {
                    label = "Git",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is GitService).SearchKeywords
                };
            return provider;
        }

        private GitService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is GitService)?.PreferencesGUI();
        }
    }
}
