using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class ItchioService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new ItchioService_Preferences_Provider("Preferences/Build Uploader/Services/Itchio", SettingsScope.User)
                {
                    label = "Itchio",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is ItchioService).SearchKeyworks
                };
            return provider;
        }
        
        private ItchioService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is ItchioService)?.PreferencesGUI();
        }
    }
}