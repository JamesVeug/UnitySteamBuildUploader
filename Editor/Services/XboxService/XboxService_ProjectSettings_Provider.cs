using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class XboxService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new XboxService_ProjectSettings_Provider(
                "Project/Build Uploader/Services/Xbox", SettingsScope.Project)
            {
                label    = "Xbox",
                keywords = InternalUtils.AllServices()
                    .FirstOrDefault(a => a is XboxService)?.SearchKeywords
            };
            return provider;
        }

        private XboxService_ProjectSettings_Provider(
            string path,
            SettingsScope scopes,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is XboxService)?.ProjectSettingsGUI();
        }
    }
}
