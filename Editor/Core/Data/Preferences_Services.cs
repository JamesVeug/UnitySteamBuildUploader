using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class PreferencesServices : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateServicesPreferencesProvider()
        {
            var provider =
                new PreferencesServices("Preferences/Build Uploader/Services", SettingsScope.User)
                {
                    label = "Services",
                    keywords = new HashSet<string>(new[]
                    {
                        "Steam", "Build", "Uploader", "Pipe", "line", "Github", "Cache", "Itchio", "Itch.io",
                    })
                };

            return provider;
        }

        private PreferencesServices(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            GUILayout.Label("Preferences for the Build Uploader that exists per user and not shared.", EditorStyles.wordWrappedLabel);

            foreach (AService service in InternalUtils.AllServices())
            {
                GUILayout.Space(20);
                service.PreferencesGUI();
            }
        }
    }
}