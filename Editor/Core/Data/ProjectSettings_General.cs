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
                "Build", "Uploader", "Build Uploader", "BuildUploader", "Pipe", "line", "pipe line", "Pipeline", "meta", "data"
            });
            
            var provider =
                new ProjectSettings_Settings("Project/Build Uploader/General", SettingsScope.Project)
                {
                    label = "General",
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
            
            EditorGUILayout.Space(20);
            
            BuildUploaderProjectSettings settings = BuildUploaderProjectSettings.Instance;

            GUILayout.Label("Build Meta Data", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Include Streaming Folder", "Include build number and more information in the streaming folder to be referenced in your project.");
                GUILayout.Label(label, GUILayout.MaxWidth(150));

                bool newInclude = GUILayout.Toggle(settings.IncludeBuildMetaDataInStreamingDataFolder, "");
                if (newInclude != settings.IncludeBuildMetaDataInStreamingDataFolder)
                {
                    settings.IncludeBuildMetaDataInStreamingDataFolder = newInclude;
                    BuildUploaderProjectSettings.Save();
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Last Build Number", "Number of the last build created using the build uploader.");
                GUILayout.Label(label, GUILayout.MaxWidth(150));
                
                int newBuildNumber = EditorGUILayout.IntField(settings.LastBuildNumber);
                if (newBuildNumber != settings.LastBuildNumber)
                {
                    settings.LastBuildNumber = newBuildNumber;
                    BuildUploaderProjectSettings.Save();
                }
            }
        }
    }
}