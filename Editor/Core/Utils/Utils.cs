using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wireframe
{
    public static class Utils
    {
        public static readonly string CacheFolder = Application.persistentDataPath + "/BuildUploader/CachedBuilds";
        
        public static Texture2D WindowIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/Icon.png";
                Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Object));
                return loadAssetAtPath as Texture2D;
            }
        }
        
        public static Texture2D WarningIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/warningicon.png";
                Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Object));
                return loadAssetAtPath as Texture2D;
            }
        }
        
        public static bool IsPathADirectory(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                return (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }
        
        public static async Task<bool> CopyDirectoryAsync(string sourcePath, string cacheFolderPath)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, cacheFolderPath));
                }

                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    await CopyFileAsync(newPath, newPath.Replace(sourcePath, cacheFolderPath));
                }

                return true;
            }
            catch (IOException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static string TruncateText(string m_enteredFilePath, float maxWidth, string defaultText)
        {
            string displayedPath = m_enteredFilePath;
            if (!string.IsNullOrEmpty(displayedPath))
            {
                float characterWidth = 8f;
                int characters = displayedPath.Length;
                float expectedWidth = characterWidth * characters;
                if (expectedWidth >= maxWidth)
                {
                    int charactersToRemove = (int)((expectedWidth - maxWidth) / characterWidth);
                    if (charactersToRemove < displayedPath.Length)
                    {
                        displayedPath = displayedPath.Substring(charactersToRemove);
                    }
                    else
                    {
                        displayedPath = "";
                    }
                }
            
                if(displayedPath.Length < m_enteredFilePath.Length)
                {
                    displayedPath = "..." + displayedPath;
                }
            }
            else
            {
                displayedPath = defaultText;
            }

            return displayedPath;
        }
    }
}