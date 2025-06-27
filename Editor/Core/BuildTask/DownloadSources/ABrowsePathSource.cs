using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a path of some sort to upload
    /// </summary>
    public abstract partial class ABrowsePathSource : ABuildSource
    {
        protected enum PathType
        {
            [Wiki(nameof(Absolute), "Specify the full path of a file or folder.")]
            Absolute,
            
            [Wiki(nameof(PathToAssets), "Specify the path starting from the projects Assets folder.")]
            PathToAssets,
        }
        
        [Wiki("Path", "The path to the file or folder to upload.")]
        protected string m_enteredFilePath = "";
        
        [Wiki("Path Type", "Specify which directory to select the content from.")]
        private PathType m_pathType;

        private string m_finalSourcePath = "";

        public ABrowsePathSource() : base()
        {
            // Required for reflection
        }

        public ABrowsePathSource(string path) : base()
        {
            m_enteredFilePath = path;
        }

        public bool SetNewPath(string newPath)
        {
            if (newPath == m_enteredFilePath || string.IsNullOrEmpty(newPath))
            {
                return false;
            }
            
            m_enteredFilePath = newPath;
            return true;
        }

        public bool PathExists()
        {
            string path = GetFullPath();
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }
            
            return File.Exists(path) || Directory.Exists(path);
        }

        public override Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult)
        {
            // Decide where we want to download to
            m_progressDescription = "Preparing...";
            m_downloadProgress = 0;
            string directoryPath = Preferences.CacheFolderPath;
            if (!Directory.Exists(directoryPath))
            {
                stepResult.AddLog("Creating cache directory: " + directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            // Make copy to avoid sharing conflicts
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            string sourcePath = GetFullPath();
            bool isDirectory = Utils.IsPathADirectory(sourcePath);
            if (!isDirectory && sourcePath.EndsWith(".exe"))
            {
                // Given a .exe. Use the Folder because they likely want to upload the entire folder - not just the .exe
                sourcePath = Path.GetDirectoryName(sourcePath);
            }
            
            m_finalSourcePath = sourcePath;
            m_progressDescription = "Done!";
            return Task.FromResult(true);
        }

        private string GetFullPath()
        {
            string path = GetSubPath();
            if (string.IsNullOrEmpty(path))
            {
                return m_enteredFilePath;
            }

            return Path.Combine(path, m_enteredFilePath);
        }

        private string GetSubPath()
        {
            switch (m_pathType)
            {
                case PathType.Absolute:
                    return "";
                case PathType.PathToAssets:
                    return Application.dataPath;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string SourceFilePath()
        {
            return m_finalSourcePath;
        }

        public override float DownloadProgress()
        {
            return m_downloadProgress;
        }

        public override string ProgressTitle()
        {
            return "Getting " + DisplayName;
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            string path = GetFullPath();
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                errors.Add("Path does not exist");
            }
        }

        public override string GetBuildDescription()
        {
            // Windows #44 Release
            string description = "";

            string fileName = Path.GetFileNameWithoutExtension(m_enteredFilePath);
            if (fileName.Contains("windows"))
            {
                description += "Windows ";
            }
            else if (fileName.Contains("windows"))
            {
                description += "Mac ";
            }

            if (fileName.LastIndexOf("-") > 0)
            {
                if (int.TryParse(fileName.Substring(fileName.LastIndexOf("-") + 1), out int i))
                {
                    description += "#" + i + " ";
                }
            }

            if (fileName.Contains("development"))
            {
                description += "Dev";
            }
            else
            {
                description += "Release";
            }

            return description;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>()
            {
                { "enteredFilePath", m_enteredFilePath },
                { "pathType", (long)m_pathType }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("enteredFilePath", out object p))
            {
                m_enteredFilePath = (string)p;
            }
            
            if (data.TryGetValue("pathType", out object pt))
            {
                m_pathType = (PathType)(long)pt;
            }
            else
            {
                m_pathType = PathType.Absolute;
            }
        }
    }
}