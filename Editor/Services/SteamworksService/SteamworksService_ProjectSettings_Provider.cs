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
                new SteamworksService_ProjectSettings_Provider("Project/Build Uploader/Services/Steamworks", SettingsScope.Project)
                {
                    label = "Steamworks",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is SteamworksService).SearchKeywords
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