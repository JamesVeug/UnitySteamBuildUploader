using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wireframe
{
    public static class Utils
    {
        public enum FileExistHandling
        {
            Error,
            Skip,
            Overwrite,
        }
        
        public static Texture2D WindowIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/Icon.png";
                Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Object));
                return loadAssetAtPath as Texture2D;
            }
        }
        
        public static Texture2D ErrorIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/erroricon.png";
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

        public static async Task<bool> CopyFileAsync(string source, string destination, FileExistHandling dupeFileHandling, UploadTaskReport.StepResult result = null)
        {
            try{
                if (destination.Length > MaxFilePath)
                {
                    throw new FilePathTooIsLongException();
                }
                
                if (File.Exists(destination))
                {
                    switch (dupeFileHandling)
                    {
                        case FileExistHandling.Error:
                            result?.AddError("File already exists: " + destination);
                            result?.SetFailed("File already exists: " + destination);
                            return false;
                        case FileExistHandling.Skip:
                            result?.AddLog("Skipping duplicate file since it already exists: " + destination);
                            return true;
                    }
                }
                
                using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    using (var destinationStream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
            }
            catch (Exception e)
            {
                if (result != null)
                {
                    result.AddException(e);
                    result.SetFailed("Failed to copy directory: " + source + " to " + destination);
                }
                else
                {
                    Debug.LogException(e);
                }
                return false;
            }

            return true;
        }
        
        public static async Task<bool> CopyDirectoryAsync(string source, string destination, FileExistHandling dupeFileHandling, UploadTaskReport.StepResult result = null, Func<string, bool> ignore = null)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                {
                    if (ignore != null && ignore(dirPath))
                    {
                        continue;
                    }

                    string newDirectory = dirPath.Replace(source, destination);
                    if (newDirectory.Length > MaxFilePath)
                    {
                        throw new FilePathTooIsLongException();
                    }
                    
                    Directory.CreateDirectory(newDirectory);
                }

                foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                {
                    if (ignore != null && ignore(newPath))
                    {
                        continue;
                    }
                    
                    string destinationFile = newPath.Replace(source, destination);
                    if (destinationFile.Length > MaxFilePath)
                    {
                        throw new FilePathTooIsLongException();
                    }
                    
                    string directory = Path.GetDirectoryName(destinationFile);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    if (!await CopyFileAsync(newPath, destinationFile, dupeFileHandling, result))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (IOException e)
            {
                if (result != null)
                {
                    result.AddException(e);
                    result.SetFailed("Failed to copy directory: " + source + " to " + destination);
                }
                else
                {
                    Debug.LogException(e);
                }

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
        
        public static string Replace(string text, string otherString, string with, StringComparison compare)
        {
#if UNITY_2021_1_OR_NEWER
            return text.Replace(otherString, with, compare);
#else
            int index = text.IndexOf(otherString, compare);
            while(index >= 0)
            {
                text = text.Remove(index, otherString.Length).Insert(index, with);
                index = text.IndexOf(otherString, index + with.Length, compare);
            }
            return text;
#endif
        }
        
        public static bool Contains(string text, string otherString, StringComparison compare)
        {
#if UNITY_2021_1_OR_NEWER
            return text.Contains(otherString, compare);
#else
            return text.IndexOf(otherString, compare) >= 0;
#endif
        }
        
        public static List<string> GetSortedFilesAndDirectories(string directory)
        {
            if (File.Exists(directory)) 
            {
                List<string> singleFile = new List<string>();
                singleFile.Add(directory);
                return singleFile;
            }
            
            // Log out every file in this directory
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            string[] folders = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);

            List<string> allFiles = files.Concat(folders).Distinct().ToList();
            allFiles.Sort(CompareFileNames);

            return allFiles;
        }

        private static int CompareFileNames(string a, string b)
        {
            // Order by folder depth first, then by name
            var aKey = a.Split(Path.DirectorySeparatorChar);
            int aLength = aKey.Length;

            var bKey = b.Split(Path.DirectorySeparatorChar);
            int bLength = bKey.Length;

            for (int aIndex = 0; aIndex < Mathf.Min(aKey.Length, bKey.Length); aIndex++)
            {
                string strA = bKey[aIndex];
                string strB = aKey[aIndex];
                int compare = string.Compare(strA, strB, StringComparison.Ordinal);
                if (compare != 0)
                {
                    return compare;
                }
            }

            if (aLength != bLength)
            {
                return aLength - bLength;
            }

            return string.Compare(a, b, StringComparison.Ordinal);
        }

        public static bool CreateInstance<T>(Type type, out T result)
        {
            if (type == null)
            {
                result = default(T);
                return false;
            }
            
            ConstructorInfo ci = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, Type.EmptyTypes, null);
            if (ci != null)
            {
                result = (T)ci.Invoke(null);
                return true;
            }

            Debug.LogError($"Could not create Type {type}. Missing empty constructor.");
            result = default(T);
            return false;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/7140575/mac-os-x-lion-what-is-the-max-path-length
        /// </summary>
        public static int MaxFilePath
        {
            get
            {
#if UNITY_EDITOR_WIN
                return 260; // Windows
#elif UNITY_EDITOR_OSX
                return 255; // macOS (or 1024 for HFS+ if you believe
#elif UNITY_EDITOR_LINUX
                return 4096; // macOS (or 255 for HFS+ if you believe Apple)
#else
                return 4096; // unknown but this seems random and
#endif
            }
        }
    }
}