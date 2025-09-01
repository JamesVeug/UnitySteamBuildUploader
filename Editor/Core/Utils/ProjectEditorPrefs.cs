using UnityEngine;
using UnityEngine.Windows;

namespace Wireframe
{
    public static class ProjectEditorPrefs
    {
        public static readonly string ProjectID;
        private static readonly string ProjectKeySuffix;

        static ProjectEditorPrefs()
        {
            ProjectID = GetProjectID();
            ProjectKeySuffix = $"{ProjectID}_";
        }
        
        /// <summary>
        /// Unity doesn't have a unique ID for a project unless you use the cloud which is useless
        /// So I'm making a custom file 
        /// </summary>
        /// <returns></returns>
        private static string GetProjectID()
        {
            string ProjectIDFilePath = Application.dataPath + "/../BuildUploader/ProjectID.txt";
            if (System.IO.File.Exists(ProjectIDFilePath))
            {
                return System.IO.File.ReadAllText(ProjectIDFilePath);
            }

            string newID = System.Guid.NewGuid().ToString();
            string directoryName = System.IO.Path.GetDirectoryName(ProjectIDFilePath);
            if (!Directory.Exists(directoryName))
            {
                System.IO.Directory.CreateDirectory(directoryName);
            }

            System.IO.File.WriteAllText(ProjectIDFilePath, newID);
            return newID;
        }

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

        public enum PrefType
        {
            Bool,
            Int,
            String,
            Float
        }
        
        public static void MigrateFromEditorPrefs(string key, PrefType prefType)
        {
            if (!UnityEditor.EditorPrefs.HasKey(key))
            {
                // Debug.Log($"[{key}] Key doesn't exist in editor prefs, nothing to migrate");
                return;
            }

            string projectKey = ProjectKeySuffix + key;
            if (HasKey(projectKey))
            {
                // Already migrated
                // Debug.Log($"[{key}] Key already migrated. deleting from editor prefs");
                UnityEditor.EditorPrefs.DeleteKey(key);
                return;
            }

            switch (prefType)
            {
                case PrefType.Bool:
                    bool boolValue = UnityEditor.EditorPrefs.GetBool(key);
                    SetBool(key, boolValue);
                    break;
                case PrefType.Int:
                    int intValue = UnityEditor.EditorPrefs.GetInt(key);
                    SetInt(key, intValue);
                    break;
                case PrefType.String:
                    string stringValue = UnityEditor.EditorPrefs.GetString(key);
                    SetString(key, stringValue);
                    break;
                case PrefType.Float:
                    float floatValue = UnityEditor.EditorPrefs.GetFloat(key);
                    SetFloat(key, floatValue);
                    break;
                default:
                    throw new System.NotImplementedException(prefType.ToString());
            }

            UnityEditor.EditorPrefs.DeleteKey(key);
            // Debug.Log($"[{key}] Migrated to project editor prefs");
        }
    }
}