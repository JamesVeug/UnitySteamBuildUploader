using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EMailService
    {
        private static ReorderableListOfEMailAccountsPreferences _reorderableListOfEMailAccountsPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Sets per-machine SMTP credentials for each account. For Gmail use an App Password.");
                if (GUILayout.Button("Gmail App Passwords", GUILayout.Width(170)))
                {
                    Application.OpenURL("https://myaccount.google.com/apppasswords");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EMail.Enabled = GUILayout.Toggle(EMail.Enabled, "Enabled");
                if (!EMail.Enabled)
                {
                    return;
                }

                EMailConfig config = EMailUIUtils.GetConfig();
                if (_reorderableListOfEMailAccountsPreferences == null)
                {
                    _reorderableListOfEMailAccountsPreferences = new ReorderableListOfEMailAccountsPreferences();
                    _reorderableListOfEMailAccountsPreferences.Initialize(config.accounts, "Accounts",
                        true, (_) =>
                        {
                            EMailUIUtils.AccountPopup.Refresh();
                            EMailUIUtils.Save();
                        });
                }

                if (_reorderableListOfEMailAccountsPreferences.OnGUI())
                {
                    EMailUIUtils.AccountPopup.Refresh();
                    EMailUIUtils.Save();
                }

                GUILayout.Label("Server, Port and From identity are configured under Project Settings -> Build Uploader -> Services -> EMail.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}
