using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class NintendoService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new NintendoService_ProjectSettings_Provider("Project/Build Uploader/Services/Nintendo", SettingsScope.Project)
                {
                    label = "Nintendo",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a=>a is NintendoService).SearchKeywords
                };
            return provider;
        }

        private NintendoService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a=>a is NintendoService)?.ProjectSettingsGUI();
        }
    }
}
