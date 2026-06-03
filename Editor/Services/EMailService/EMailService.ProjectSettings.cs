using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EMailService
    {
        private static ReorderableListOfEMailAccountsProjectSettings _reorderableListOfEMailAccountsProjectSettings;

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                EMailConfig config = EMailUIUtils.GetConfig();

                GUILayout.Label("Accounts", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Server, Port and From identity are shared with the team via JSON.");
                }

                if (_reorderableListOfEMailAccountsProjectSettings == null)
                {
                    _reorderableListOfEMailAccountsProjectSettings = new ReorderableListOfEMailAccountsProjectSettings();
                    _reorderableListOfEMailAccountsProjectSettings.Initialize(config.accounts, "Accounts",
                        true, (_) =>
                        {
                            EMailUIUtils.AccountPopup.Refresh();
                            EMailUIUtils.Save();
                        });
                }

                if (_reorderableListOfEMailAccountsProjectSettings.OnGUI())
                {
                    EMailUIUtils.AccountPopup.Refresh();
                    EMailUIUtils.Save();
                }

                GUILayout.Label("Username and Password are entered per-machine under Preferences -> Build Uploader -> Services -> EMail.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}
