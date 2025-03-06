using UnityEngine;
using UnityEditor;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Wireframe
{
    internal static class Preferences
    {
        public static bool DeleteCacheAfterBuild
        {
            get => EditorPrefs.GetBool("BuildUploader_DeleteCacheAfterBuild", true);
            set => EditorPrefs.SetBool("BuildUploader_DeleteCacheAfterBuild", value);
        } 
        
        [PreferenceItem("Build Uploader")]
        private static void OnPreferencesGUI()
        {
            GUILayout.Label("Preferences for the Build Uploader. Required to log into various services.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Cached Builds");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Cache Folder"))
                {
                    EditorUtility.RevealInFinder(Utils.CacheFolder);
                }
                
                EditorGUILayout.LabelField("Delete cache after uploading", GUILayout.Width(170));

                bool cacheAfterBuild = DeleteCacheAfterBuild;
                bool newCacheAfterBuild = EditorGUILayout.Toggle(cacheAfterBuild);
                if (newCacheAfterBuild != DeleteCacheAfterBuild)
                {
                    DeleteCacheAfterBuild = newCacheAfterBuild;
                }
            }

            foreach (AService service in InternalUtils.AllServices())
            {
                GUILayout.Space(20);
                service.PreferencesGUI();
            }

            GUILayout.Space(20);
        }
    }
}