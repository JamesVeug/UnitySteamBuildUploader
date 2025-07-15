using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class GithubService
    {
        public override void PreferencesGUI()
        {
            GUILayout.Label("Github", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                Github.Enabled = GUILayout.Toggle(Github.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!Github.Enabled))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Github.Token = PasswordField.Draw("Token:", "Secret required to access your Github account", 105, Github.Token, onHelpPressed:TokenHelp);
                    }
                }
            }
        }

        private static void TokenHelp()
        {
            Application.OpenURL("https://github.com/settings/tokens");
        }
    }
}