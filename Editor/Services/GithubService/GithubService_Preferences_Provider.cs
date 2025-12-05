using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class GithubService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new GithubService_Preferences_Provider("Preferences/Build Uploader/Services/Github", SettingsScope.User)
                {
                    label = "Github",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is GithubService).SearchKeyworks
                };
            return provider;
        }
        
        private GithubService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is GithubService)?.PreferencesGUI();
        }
    }
}