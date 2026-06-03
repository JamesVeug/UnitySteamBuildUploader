using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class NintendoService_Preferences_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new NintendoService_Preferences_Provider("Preferences/Build Uploader/Services/Nintendo", SettingsScope.User)
                {
                    label = "Nintendo",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is NintendoService).SearchKeywords
                };
            return provider;
        }

        private NintendoService_Preferences_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is NintendoService)?.PreferencesGUI();
        }
    }
}
