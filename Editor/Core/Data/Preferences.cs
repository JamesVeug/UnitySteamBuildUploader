using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class Preferences : SettingsProvider
    {
        private static readonly string DefaultCacheFolder = Application.persistentDataPath + "/BuildUploader/CachedBuilds";

        public enum ShowIf
        {
            Always,
            Never,
            Successful,
            Failed,
        }
        
        public static bool DeleteCacheAfterUpload
        {
            get => EditorPrefs.GetBool("BuildUploader_DeleteCacheAfterBuild", true);
            set => EditorPrefs.SetBool("BuildUploader_DeleteCacheAfterBuild", value);
        }
        
        public static ShowIf ShowConfirmationWindowAfterUpload
        {
            get => (ShowIf)EditorPrefs.GetInt("BuildUploader_ShowConfirmationWindowAfterUpload", (int)ShowIf.Always);
            set => EditorPrefs.SetInt("BuildUploader_ShowConfirmationWindowAfterUpload", (int)value);
        }
        
        public static bool AutoSaveReportToCacheFolder
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoSaveReportToCacheFolder", false);
            set => EditorPrefs.SetBool("BuildUploader_AutoSaveReportToCacheFolder", value);
        }
        
        public static bool AutoSaveBuildConfigsAfterChanges
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoSaveBuildConfigsAfterChanges", true);
            set => EditorPrefs.SetBool("BuildUploader_AutoSaveBuildConfigsAfterChanges", value);
        }
        
        public static ShowIf ShowReportAfterUpload
        {
            get => (ShowIf)EditorPrefs.GetInt("BuildUploader_ShowReportAfterUpload", (int)ShowIf.Always);
            set => EditorPrefs.SetInt("BuildUploader_ShowReportAfterUpload", (int)value);
        }
        
        public static bool AutoDecompressZippedSourceFiles
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoDecompressZippedSourceFiles", true);
            set => EditorPrefs.SetBool("BuildUploader_AutoDecompressZippedSourceFiles", value);
        }
        
        public static string DefaultDescriptionFormat
        {
            get => EditorPrefs.GetString("BuildUploader_DefaultDescriptionFormat", "v{version} - ");
            set => EditorPrefs.SetString("BuildUploader_DefaultDescriptionFormat", value);
        }
        
        public static string CacheFolderPath
        {
            get => ProjectEditorPrefs.GetString("BuildUploader_CacheFolderPath", DefaultCacheFolder);
            set => ProjectEditorPrefs.SetString("BuildUploader_CacheFolderPath", value);
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
            EditorGUILayout.LabelField(new GUIContent("Cached Builds",
                    "When starting an upload all source files will be copied to a temporary location to avoid modifying raw files. This is known as the cache."), 
                EditorStyles.boldLabel);
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
                EditorGUILayout.LabelField(
                    new GUIContent("Delete cache after uploading",
                        "If enabled, the cache folder of a build will be deleted after completion. This is useful to save space."),
                    GUILayout.Width(200));

                bool cacheAfterBuild = DeleteCacheAfterUpload;
                bool newCacheAfterBuild = EditorGUILayout.Toggle(cacheAfterBuild);
                if (newCacheAfterBuild != DeleteCacheAfterUpload)
                {
                    DeleteCacheAfterUpload = newCacheAfterBuild;
                }
            }

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Default Description:", 
                        "Description in the Upload tab to start with when opened." +
                        "\n\nFormats:" +
                        "\n{version} will be replaced with the version of the build. eg: 1.0.0"), 
                    GUILayout.Width(200));

                string descFormat = DefaultDescriptionFormat;
                string newDescFormat = EditorGUILayout.TextField(descFormat);
                if (newDescFormat != DefaultDescriptionFormat)
                {
                    DefaultDescriptionFormat = newDescFormat;
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Auto Save Build Configs", 
                        "If enabled, after every change made to build configs in the upload tab they will be saved. If disabled then requires pressing the save button to retain your changes."), 
                    GUILayout.Width(200));

                bool autoSave = AutoSaveBuildConfigsAfterChanges;
                bool newAutoSave = EditorGUILayout.Toggle(autoSave);
                if (newAutoSave != AutoSaveBuildConfigsAfterChanges)
                {
                    AutoSaveBuildConfigsAfterChanges = newAutoSave;
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Auto decompress source .zips", 
                        "If enabled, a Source that contains only a .zip it will be decompressed when being copied over to the cache."), 
                    GUILayout.Width(200));

                bool autoDecompress = AutoDecompressZippedSourceFiles;
                bool newAutoDecompress = EditorGUILayout.Toggle(autoDecompress);
                if (newAutoDecompress != AutoDecompressZippedSourceFiles)
                {
                    AutoDecompressZippedSourceFiles = newAutoDecompress;
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Auto save build reports", "" +
                                                              "If enabled, build reports made from the UI will be auto-saved to the cache folder after completion."),
                    GUILayout.Width(200));

                bool autoSave = AutoSaveReportToCacheFolder;
                bool newAutoSave = EditorGUILayout.Toggle(autoSave);
                if (newAutoSave != AutoSaveReportToCacheFolder)
                {
                    AutoSaveReportToCacheFolder = newAutoSave;
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Show upload confirmation window", 
                        "If enabled, a popup window will appear indicating if the upload was successful and if not why not."), 
                    GUILayout.Width(200));

                ShowIf showConfirmations = ShowConfirmationWindowAfterUpload;
                ShowIf newShowConfirmations = (ShowIf)EditorGUILayout.EnumPopup(showConfirmations);
                if (newShowConfirmations != ShowConfirmationWindowAfterUpload)
                {
                    ShowConfirmationWindowAfterUpload = newShowConfirmations;
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Show Build report after uploading", "" +
                                                              "If enabled, when an upload completes a window will appear showing all information about what it did."),
                    GUILayout.Width(200));

                ShowIf showReport = ShowReportAfterUpload;
                ShowIf newShowReport = (ShowIf)EditorGUILayout.EnumPopup(showReport);
                if (newShowReport != ShowReportAfterUpload)
                {
                    ShowReportAfterUpload = newShowReport;
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