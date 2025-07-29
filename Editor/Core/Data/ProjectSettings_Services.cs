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
                new ProjectSettings_Services("Project/BuildUploader/Services", SettingsScope.Project)
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
    
            foreach (AService service in InternalUtils.AllServices().OrderBy(a=>a.GetType().Name))
            {
                if (service.HasProjectSettingsGUI)
                {
                    GUILayout.Label(service.GetType().Name, EditorStyles.whiteLargeLabel);
                    service.ProjectSettingsGUI();
                }
            }
        }
    }
}