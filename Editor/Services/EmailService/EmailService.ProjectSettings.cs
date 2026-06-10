using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EmailService
    {
        private static ReorderableListOfEmailAccountsProjectSettings _reorderableListOfEmailAccountsProjectSettings;

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                EmailConfig config = EmailUIUtils.GetConfig();

                GUILayout.Label("Accounts", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Server, Port and From identity are shared with the team via JSON.");
                }

                if (_reorderableListOfEmailAccountsProjectSettings == null)
                {
                    _reorderableListOfEmailAccountsProjectSettings = new ReorderableListOfEmailAccountsProjectSettings();
                    _reorderableListOfEmailAccountsProjectSettings.Initialize(config.accounts, "Accounts",
                        true, (_) =>
                        {
                            EmailUIUtils.AccountPopup.Refresh();
                            EmailUIUtils.Save();
                        });
                }

                if (_reorderableListOfEmailAccountsProjectSettings.OnGUI())
                {
                    EmailUIUtils.AccountPopup.Refresh();
                    EmailUIUtils.Save();
                }

                GUILayout.Label("Username and Password are entered per-machine under Preferences -> Build Uploader -> Services -> Email.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}
