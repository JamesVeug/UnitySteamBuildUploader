using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class UnityCloud
    {
        public static UnityCloud Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new UnityCloud();
                }

                return m_instance;
            }
        }

        private static UnityCloud m_instance;

        static UnityCloud()
        {
            // Migrate everything over to encoded values
            EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_organization", OrganizationKey);
            EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_project", ProjectKey);
            EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_secret", SecretKey);
        }
        
        public static bool Enabled
        {
            get => EditorPrefs.GetBool("unityCloud_enabled", false);
            set => EditorPrefs.SetBool("unityCloud_enabled", value);
        }

        private static string OrganizationKey => Application.productName + "UnityCloudOBuildUploader";
        public string Organization
        {
            get => EncodedEditorPrefs.GetString(OrganizationKey, "");
            set => EncodedEditorPrefs.SetString(OrganizationKey, value);
        }

        private static string ProjectKey => Application.productName + "UnityCloudPBuildUploader";
        public string Project
        {
            get => EncodedEditorPrefs.GetString(ProjectKey, "");
            set => EncodedEditorPrefs.SetString(ProjectKey, value);
        }

        private static string SecretKey => Application.productName + "UnityCloudSBuildUploader";
        public string Secret
        {
            get => EncodedEditorPrefs.GetString(SecretKey, "");
            set => EncodedEditorPrefs.SetString(SecretKey, value);
        }

        public bool IsInitialized()
        {
            return !string.IsNullOrEmpty(Organization) && !string.IsNullOrEmpty(Project) &&
                   !string.IsNullOrEmpty(Secret);
        }
    }
}