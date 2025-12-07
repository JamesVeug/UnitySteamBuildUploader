using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class EpicGamesService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new EpicGamesService_ProjectSettings_Provider("Project/Build Uploader/Services/Epic Games", SettingsScope.Project)
                {
                    label = "Epic Games",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is EpicGamesService).SearchKeywords
                };
            return provider;
        }
        
        private EpicGamesService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is EpicGamesService)?.ProjectSettingsGUI();
        }
    }
}