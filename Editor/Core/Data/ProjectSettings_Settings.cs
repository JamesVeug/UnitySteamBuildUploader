using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Services tab for Project settings
    /// </summary>
    public partial class ProjectSettings_Settings : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            HashSet<string> keywords = new HashSet<string>(new[]
            {
                "Build", "Uploader", "Pipe", "line", "settings" 
            });
            
            var provider =
                new ProjectSettings_Settings("Project/BuildUploader", SettingsScope.Project)
                {
                    label = "BuildUploader",
                    keywords = keywords
                };
    
            return provider;
        }
    
        private ProjectSettings_Settings(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            GUILayout.Label("Settings for the Build Uploader that exists per project and shared with all users with access to your Unity Project via version control.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("To prevent them being shared by version control add `BuildUploader` to your .gitignore", EditorStyles.wordWrappedLabel);
            GUILayout.Label("All settings will not be accessible in builds.", EditorStyles.wordWrappedLabel);
        }
    }
}