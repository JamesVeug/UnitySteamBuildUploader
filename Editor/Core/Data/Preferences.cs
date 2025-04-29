using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class Preferences : SettingsProvider
    {
        public static bool DeleteCacheAfterBuild
        {
            get => EditorPrefs.GetBool("BuildUploader_DeleteCacheAfterBuild", true);
            private set => EditorPrefs.SetBool("BuildUploader_DeleteCacheAfterBuild", value);
        }
        
        public static bool AutoDecompressZippedSourceFiles
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoDecompressZippedSourceFiles", true);
            private set => EditorPrefs.SetBool("BuildUploader_AutoDecompressZippedSourceFiles", value);
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider =
                new Preferences("Preferences/Build Uploader", SettingsScope.User)
                {
                    label = "Build Uploader",
                    keywords = new HashSet<string>(new[]
                    {
                        "Steam", "Build", "Uploader", "Pipe", "line", "Github", "Cache"
                    })
                };

            return provider;
        }

        private Preferences(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            GUILayout.Label("Preferences for the Build Uploader. Required to log into various services.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Cached Builds", EditorStyles.boldLabel);
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
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Auto decompress .zip files", GUILayout.Width(170));

                bool autoDecompress = AutoDecompressZippedSourceFiles;
                bool newAutoDecompress = EditorGUILayout.Toggle(autoDecompress);
                if (newAutoDecompress != AutoDecompressZippedSourceFiles)
                {
                    AutoDecompressZippedSourceFiles = newAutoDecompress;
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