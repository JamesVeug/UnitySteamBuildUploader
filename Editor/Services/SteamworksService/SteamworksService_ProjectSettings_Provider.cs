using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class SteamworksService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new SteamworksService_ProjectSettings_Provider("Project/BuildUploader/Services/Steamworks", SettingsScope.Project)
                {
                    label = "Steamworks",
                    keywords = new HashSet<string>(new[]
                    {
                        "Build", "Uploader", "Pipe", "line", "service", "Steam", "Steamworks"
                    })
                };
            return provider;
        }
        
        private SteamworksService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is SteamworksService)?.ProjectSettingsGUI();
        }
    }
} 