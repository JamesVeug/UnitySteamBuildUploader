using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class EncodedEditorPrefs
    {
        public static void MigrateKeyToEncoded<T>(string oldKey, string newKey)
        {
            if (!EditorPrefs.HasKey(oldKey))
            {
                return;
            }
            
            string encodedNewKey = EncodedValue<string>.Encode64(newKey);
            if (typeof(T) == typeof(bool))
            {
                bool unencodedValue = EditorPrefs.GetBool(oldKey);
                string encodedValue = EncodedValue<bool>.Encode64(unencodedValue);
                EditorPrefs.SetString(encodedNewKey, encodedValue);
            }
            else if (typeof(T) == typeof(string))
            {
                string unencodedValue = EditorPrefs.GetString(oldKey);
                string encodedValue = EncodedValue<string>.Encode64(unencodedValue);
                EditorPrefs.SetString(encodedNewKey, encodedValue);
            }
            else
            {
                throw new System.NotImplementedException(typeof(T).FullName);
            }
            EditorPrefs.DeleteKey(oldKey);
        }
        
        public static void SetBool(string key, bool value)
        {
            string encodedKey = EncodedValue<string>.Encode64(key);
            string encodedValue = EncodedValue<bool>.Encode64(value);
            EditorPrefs.SetString(encodedKey, encodedValue);
        }
        
        public static bool GetBool(string key, bool defaultValue)
        {
            string encodedKey = EncodedValue<string>.Encode64(key);
            string encodedDefaultValue = EncodedValue<bool>.Encode64(defaultValue);
            string encodedValue = EditorPrefs.GetString(encodedKey, encodedDefaultValue);
            return EncodedValue<bool>.Decode64(encodedValue);
        }
        
        public static void SetString(string key, string value)
        {
            string encodedKey = EncodedValue<string>.Encode64(key);
            string encodedValue = EncodedValue<string>.Encode64(value);
            EditorPrefs.SetString(encodedKey, encodedValue);
        }
        
        public static string GetString(string key, string defaultValue)
        {
            string encodedKey = EncodedValue<string>.Encode64(key);
            string encodedDefaultValue = EncodedValue<string>.Encode64(defaultValue);
            string encodedValue = EditorPrefs.GetString(encodedKey, encodedDefaultValue);
            return EncodedValue<string>.Decode64(encodedValue);
        }
        
        public static void MigrateStringKeyToKey(string oldKey, string newKey)
        {
            string encodedOldKey = EncodedValue<string>.Encode64(oldKey);
            if (!EditorPrefs.HasKey(encodedOldKey))
            {
                // Debug.Log($"[{oldKey}][{newKey}] Old key does not exist, not migrating.");
                return;
            }
            
            string encodedNewKey = EncodedValue<string>.Encode64(newKey);
            string value = EditorPrefs.GetString(encodedOldKey);
            EditorPrefs.SetString(encodedNewKey, value);
            EditorPrefs.DeleteKey(encodedOldKey);
            // Debug.Log($"[{oldKey}][{newKey}] Migrated key.");
        }
    }
}