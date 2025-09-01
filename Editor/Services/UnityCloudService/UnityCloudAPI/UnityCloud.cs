namespace Wireframe
{
    internal partial class UnityCloud
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

        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("unityCloud_enabled", false);
            set => ProjectEditorPrefs.SetBool("unityCloud_enabled", value);
        }

        private static string OrganizationKey => ProjectEditorPrefs.ProjectID + "UnityCloudOBuildUploader";
        public string Organization
        {
            get => EncodedEditorPrefs.GetString(OrganizationKey, "");
            set => EncodedEditorPrefs.SetString(OrganizationKey, value);
        }

        private static string ProjectKey => ProjectEditorPrefs.ProjectID + "UnityCloudPBuildUploader";
        public string Project
        {
            get => EncodedEditorPrefs.GetString(ProjectKey, "");
            set => EncodedEditorPrefs.SetString(ProjectKey, value);
        }

        private static string SecretKey => ProjectEditorPrefs.ProjectID + "UnityCloudSBuildUploader";
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