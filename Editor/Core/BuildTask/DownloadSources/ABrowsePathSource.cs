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
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        
        protected string m_finalSourcePath;
        protected string m_enteredFilePath;

        public ABrowsePathSource() : base()
        {
            // Required for reflection
        }

        internal ABrowsePathSource(string path) : base()
        {
            m_enteredFilePath = path;
        }

        private bool SetNewPath(string newPath)
        {
            if (newPath == m_enteredFilePath || string.IsNullOrEmpty(newPath))
            {
                return false;
            }
            
            m_enteredFilePath = newPath;
            return true;
        }

        private bool PathExists()
        {
            if (string.IsNullOrEmpty(m_enteredFilePath))
            {
                return true;
            }
            
            return File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath);
        }

        public override Task<bool> GetSource(BuildConfig buildConfig, BuildTaskReport.StepResult stepResult)
        {
            // Decide where we want to download to
            m_progressDescription = "Preparing...";
            m_downloadProgress = 0;
            string directoryPath = Utils.CacheFolder;
            if (!Directory.Exists(directoryPath))
            {
                stepResult.AddLog("Creating cache directory: " + directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            // Make copy to avoid sharing conflicts
            // If it's a directory, copy the whole thing to a folder with the same name
            // If it's a file, copy it to the directory
            string sourcePath = m_enteredFilePath;
            bool isDirectory = Utils.IsPathADirectory(sourcePath);
            if (!isDirectory && m_enteredFilePath.EndsWith(".exe"))
            {
                // Given a .exe. use the Folder because they likely want to upload the entire folder - not just the .exe
                sourcePath = Path.GetDirectoryName(m_enteredFilePath);
            }
            
            m_finalSourcePath = sourcePath;
            m_progressDescription = "Done!";
            return Task.FromResult(true);
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
            return "Getting " + ((DropdownElement)this).DisplayName;
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup(out string reason)
        {
            if (string.IsNullOrEmpty(m_enteredFilePath))
            {
                reason = "Path not set";
                return false;
            }

            if (!File.Exists(m_enteredFilePath) && !Directory.Exists(m_enteredFilePath))
            {
                reason = "Path does not exist";
                return false;
            }
            
            reason = "";
            return true;
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
                { "enteredFilePath", m_enteredFilePath }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("enteredFilePath", out object p))
            {
                m_enteredFilePath = (string)p;
            }
        }
    }
}