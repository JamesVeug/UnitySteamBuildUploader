using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class PreferencesServices : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateServicesPreferencesProvider()
        {
            HashSet<string> keywords = new HashSet<string>(new[]
            {
                "Build", "Uploader", "Pipe", "line", "Cache",
            });
            
            var provider = new PreferencesServices("Preferences/Build Uploader/Services", SettingsScope.User)
            {
                label = "Services",
                keywords = keywords,
                hasSearchInterestHandler = searchString=>
                {
                    return keywords.Any(a => Utils.Contains(a, searchString, StringComparison.OrdinalIgnoreCase));
                }
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
            foreach (AService service in InternalUtils.AllServices()
                         .Where(s=>s.MatchesSearchKeywords(searchContext))
                         .OrderBy(s=>s.ServiceName))
            {
                GUILayout.Space(20);
                GUILayout.Label(service.ServiceName, EditorStyles.boldLabel);
                service.PreferencesGUI();
            }
        }
    }
}