namespace Wireframe
{
    public class ProjectEditorPrefs
    {
        private static readonly string ProjectKeySuffix = $"{UnityEditor.PlayerSettings.companyName}_{UnityEditor.PlayerSettings.productName}_";

        public static void SetBool(string key, bool value)
        {
            UnityEditor.EditorPrefs.SetBool(ProjectKeySuffix + key, value);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return UnityEditor.EditorPrefs.GetBool(ProjectKeySuffix + key, defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            UnityEditor.EditorPrefs.SetInt(ProjectKeySuffix + key, value);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return UnityEditor.EditorPrefs.GetInt(ProjectKeySuffix + key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            UnityEditor.EditorPrefs.SetString(ProjectKeySuffix + key, value);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return UnityEditor.EditorPrefs.GetString(ProjectKeySuffix + key, defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            UnityEditor.EditorPrefs.SetFloat(ProjectKeySuffix + key, value);
        }

        public static float GetFloat(string key, float defaultValue = 0.0f)
        {
            return UnityEditor.EditorPrefs.GetFloat(ProjectKeySuffix + key, defaultValue);
        }

        public static void DeleteKey(string key)
        {
            UnityEditor.EditorPrefs.DeleteKey(ProjectKeySuffix + key);
        }

        public static bool HasKey(string key)
        {
            return UnityEditor.EditorPrefs.HasKey(ProjectKeySuffix + key);
        }
    }
}