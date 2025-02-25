using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class UnityCloudService
    {
        public override void PreferencesGUI()
        {
            GUILayout.Label("Unity Cloud", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                UnityCloud.Enabled = GUILayout.Toggle(UnityCloud.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!UnityCloud.Enabled))
                {
                    DrawUnityCloud();
                }
            }
        }

        private static void DrawUnityCloud()
        {
            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Organization =
                    PasswordField.Draw("Organization:", 105, UnityCloud.Instance.Organization);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Project = PasswordField.Draw("Project ID:", 105, UnityCloud.Instance.Project);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Secret = PasswordField.Draw("Dev Ops API Key:", 105, UnityCloud.Instance.Secret);
            }
        }
    }
}