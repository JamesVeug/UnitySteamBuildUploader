using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EmailService
    {
        private static ReorderableListOfEmailAccountsPreferences _reorderableListOfEmailAccountsPreferences;

        public override void PreferencesGUI()
        {
            base.PreferencesGUI();
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
                Email.Enabled = GUILayout.Toggle(Email.Enabled, "Enabled");
                if (!Email.Enabled)
                {
                    return;
                }

                EmailConfig config = EmailUIUtils.GetConfig();
                if (_reorderableListOfEmailAccountsPreferences == null)
                {
                    _reorderableListOfEmailAccountsPreferences = new ReorderableListOfEmailAccountsPreferences();
                    _reorderableListOfEmailAccountsPreferences.Initialize(config.accounts, "Accounts",
                        true, (_) =>
                        {
                            EmailUIUtils.AccountPopup.Refresh();
                            EmailUIUtils.Save();
                        });
                }

                if (_reorderableListOfEmailAccountsPreferences.OnGUI())
                {
                    EmailUIUtils.AccountPopup.Refresh();
                    EmailUIUtils.Save();
                }

                GUILayout.Label("Server, Port and From identity are configured under Project Settings -> Build Uploader -> Services -> Email.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }
}
