using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class Preferences : SettingsProvider
    {
        private static readonly string DefaultCacheFolder = Application.persistentDataPath + "/BuildUploader/CachedBuilds";
        
        public static bool DeleteCacheAfterBuild
        {
            get => EditorPrefs.GetBool("BuildUploader_DeleteCacheAfterBuild", true);
            private set => EditorPrefs.SetBool("BuildUploader_DeleteCacheAfterBuild", value);
        }
        
        public static bool AutoSaveReportToCacheFolder
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoSaveReportToCacheFolder", false);
            private set => EditorPrefs.SetBool("BuildUploader_AutoSaveReportToCacheFolder", value);
        }
        
        public static bool AutoDecompressZippedSourceFiles
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoDecompressZippedSourceFiles", true);
            private set => EditorPrefs.SetBool("BuildUploader_AutoDecompressZippedSourceFiles", value);
        }
        
        public static string CacheFolderPath
        {
            get => ProjectEditorPrefs.GetString("BuildUploader_CacheFolderPath", DefaultCacheFolder);
            private set => ProjectEditorPrefs.SetString("BuildUploader_CacheFolderPath", value);
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
            GUILayout.Label("Preferences for the Build Uploader that exists per user and not shared.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Cached Builds", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                string newCachePath = EditorGUILayout.TextField(CacheFolderPath);
                if (newCachePath != CacheFolderPath)
                {
                    CacheFolderPath = newCachePath;
                }
                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    string newPath = EditorUtility.OpenFolderPanel("Select Cache Folder", CacheFolderPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        CacheFolderPath = newPath;
                    }
                }
                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(CacheFolderPath);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Delete cache after uploading", GUILayout.Width(170));

                bool cacheAfterBuild = DeleteCacheAfterBuild;
                bool newCacheAfterBuild = EditorGUILayout.Toggle(cacheAfterBuild);
                if (newCacheAfterBuild != DeleteCacheAfterBuild)
                {
                    DeleteCacheAfterBuild = newCacheAfterBuild;
                }
            }

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Auto save build reports", GUILayout.Width(170));

                bool autoSave = AutoSaveReportToCacheFolder;
                bool newAutoSave = EditorGUILayout.Toggle(autoSave);
                if (newAutoSave != AutoSaveReportToCacheFolder)
                {
                    AutoSaveReportToCacheFolder = newAutoSave;
                }
            }
            
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