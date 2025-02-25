using UnityEditor;

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
        
        public static bool Enabled
        {
            get => EditorPrefs.GetBool("unityCloud_enabled", false);
            set => EditorPrefs.SetBool("unityCloud_enabled", value);
        }

        public string Organization
        {
            get => EditorPrefs.GetString("unityCloud_organization", null);
            set => EditorPrefs.SetString("unityCloud_organization", value);
        }

        public string Project
        {
            get => EditorPrefs.GetString("unityCloud_project", null);
            set => EditorPrefs.SetString("unityCloud_project", value);
        }

        public string Secret
        {
            get => EditorPrefs.GetString("unityCloud_secret", null);
            set => EditorPrefs.SetString("unityCloud_secret", value);
        }

        public bool IsInitialized()
        {
            return !string.IsNullOrEmpty(Organization) && !string.IsNullOrEmpty(Project) &&
                   !string.IsNullOrEmpty(Secret);
        }
    }
}