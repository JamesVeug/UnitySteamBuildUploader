using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Services tab for Project settings
    /// </summary>
    public class ProjectSettings_Services : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            HashSet<string> keywords = new HashSet<string>(new[]
            {
                "Build", "Uploader", "Pipe", "line", "service" 
            });
            
            var provider =
                new ProjectSettings_Services("Project/Build Uploader/Services", SettingsScope.Project)
                {
                    label = "Services",
                    keywords = keywords
                };
    
            return provider;
        }
    
        private ProjectSettings_Services(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            GUILayout.Label("Services are external APIs that the Build Uploader can use when uploading builds. Either Uploading to websites or sending messages when completed (e.g. Discord)\n", EditorStyles.wordWrappedLabel);
    
            foreach (AService service in InternalUtils.AllServices()
                         .Where(s=>s.MatchesSearchKeywords(searchContext))
                         .OrderBy(s=>s.ServiceName))
            {
                if (service.HasProjectSettingsGUI)
                {
                    GUILayout.Space(20);
                    GUILayout.Label(service.ServiceName, EditorStyles.boldLabel);
                    service.ProjectSettingsGUI();
                }
            }
        }
    }
}