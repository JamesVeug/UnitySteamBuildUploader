using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class UnityCloudService
    {
        public override void PreferencesGUI()
        {
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
                UnityCloud.Instance.Organization = PasswordField.Draw(
                    "Organization:", 
                    "The organization/account that owns the project", 
                    105, 
                    UnityCloud.Instance.Organization);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Project = PasswordField.Draw(
                    "Project ID:", 
                    "The project ID owned by the organization. eg: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", 
                    105, 
                    UnityCloud.Instance.Project);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Secret = PasswordField.Draw(
                    "Dev Ops API Key:", 
                    "The secret key used to access Unity Cloud via the API", 
                    105, 
                    UnityCloud.Instance.Secret);
            }
        }
    }
}