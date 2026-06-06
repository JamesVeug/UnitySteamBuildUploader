using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Wireframe
{
    public class EmailService_ProjectSettings_Provider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new EmailService_ProjectSettings_Provider("Project/Build Uploader/Services/Email", SettingsScope.Project)
                {
                    label = "Email",
                    keywords = InternalUtils.AllServices().FirstOrDefault(a => a is EmailService).SearchKeywords
                };
            return provider;
        }

        private EmailService_ProjectSettings_Provider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            InternalUtils.AllServices().FirstOrDefault(a => a is EmailService)?.ProjectSettingsGUI();
        }
    }
}
