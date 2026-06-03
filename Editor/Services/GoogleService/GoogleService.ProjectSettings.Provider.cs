using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class GoogleService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new GoogleService_ProjectSettings_Provider("Project/Build Uploader/Services/Google", SettingsScope.Project)
                {
                    label = "Google",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is GoogleService).SearchKeywords
                };
            return provider;
        }

        private GoogleService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is GoogleService)?.ProjectSettingsGUI();
        }
    }
}
