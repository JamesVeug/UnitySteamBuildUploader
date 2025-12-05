using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class UnityCloudService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new UnityCloudService_Preferences_Provider("Preferences/Build Uploader/Services/Unity Cloud", SettingsScope.User)
                {
                    label = "Unity Cloud",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is UnityCloudService).SearchKeyworks
                };
            return provider;
        }
        
        private UnityCloudService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is UnityCloudService)?.PreferencesGUI();
        }
    }
}