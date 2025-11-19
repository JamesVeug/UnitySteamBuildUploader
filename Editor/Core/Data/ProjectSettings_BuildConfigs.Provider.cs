using System.Collections.Generic;
using UnityEditor;

namespace Wireframe
{
    public partial class ProjectSettings_BuildConfigs
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            HashSet<string> keywords = new HashSet<string>(new[]
            {
                "Build", "Uploader", "Pipe", "line", "Config"
            });

            var provider = new ProjectSettings_BuildConfigs("Project/Build Uploader/Build Configs", SettingsScope.Project)
                {
                    label = "Build Configs",
                    keywords = keywords
                };

            return provider;
        }

        private ProjectSettings_BuildConfigs(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            BuildConfigsGUI();
        }
    }
}