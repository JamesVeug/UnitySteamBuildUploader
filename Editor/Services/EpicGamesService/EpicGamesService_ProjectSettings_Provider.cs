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
                new EpicGamesService_ProjectSettings_Provider("Project/BuildUploader/Services/EpicGames", SettingsScope.Project)
                {
                    label = "EpicGames",
                    keywords = new HashSet<string>(new[]
                    {
                        "Build", "Uploader", "Pipe", "line", "service", "Epic", "Games", "Unreal", "Engine"
                    })
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