using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class DropboxService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new DropboxService_ProjectSettings_Provider("Project/Build Uploader/Services/Dropbox", SettingsScope.Project)
                {
                    label = "Dropbox",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is DropboxService).SearchKeywords
                };
            return provider;
        }

        private DropboxService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is DropboxService)?.ProjectSettingsGUI();
        }
    }
}
