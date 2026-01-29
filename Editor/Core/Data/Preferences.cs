using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public static class Preferences
    {
        internal static readonly string DefaultCacheFolder = Application.persistentDataPath + "/BuildUploader/CachedBuilds";

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
        
        public static bool AutoFocusNewUploadTask
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoFocusNewUploadTask", true);
            set => EditorPrefs.SetBool("BuildUploader_AutoFocusNewUploadTask", value);
        }
        
        public static bool DefaultShowFormattedTextToggle
        {
            get => EditorPrefs.GetBool("BuildUploader_DefaultShowFormattedTextToggle", true);
            set => EditorPrefs.SetBool("BuildUploader_DefaultShowFormattedTextToggle", value);
        }
        
        public static bool AutoSaveReportToCacheFolder
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoSaveReportToCacheFolder", false);
            set => EditorPrefs.SetBool("BuildUploader_AutoSaveReportToCacheFolder", value);
        }
        
        public static bool AutoSaveUploadConfigsAfterChanges
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoSaveBuildConfigsAfterChanges", true);
            set => EditorPrefs.SetBool("BuildUploader_AutoSaveBuildConfigsAfterChanges", value);
        }
        
        public static bool AutoGenerateMenuItems
        {
            get => EditorPrefs.GetBool("BuildUploader_AutoGenerateMenuItems", false);
            set => EditorPrefs.SetBool("BuildUploader_AutoGenerateMenuItems", value);
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
    }
}