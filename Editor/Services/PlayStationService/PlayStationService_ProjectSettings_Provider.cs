using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class PlayStationService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new PlayStationService_ProjectSettings_Provider("Project/Build Uploader/Services/PlayStation", SettingsScope.Project)
                {
                    label = "PlayStation",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is PlayStationService).SearchKeywords
                };
            return provider;
        }

        private PlayStationService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is PlayStationService)?.ProjectSettingsGUI();
        }
    }
}
