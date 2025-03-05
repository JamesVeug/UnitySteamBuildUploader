using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a path of some sort to upload
    /// </summary>
    internal abstract class ABrowsePathSource : ABuildSource
    {
        protected abstract string ButtonText { get; }
        
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        
        protected string m_finalSourcePath;
        protected string m_enteredFilePath;
        
        public ABrowsePathSource(BuildUploaderWindow window) : base(window)
        {
            
        }

        private void Setup()
        {
            m_pathButtonExistsStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle.normal.textColor = Color.red;
            
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.red;
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            Setup();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Path:", GUILayout.Width(120));
                
                bool exists = !string.IsNullOrEmpty(m_enteredFilePath) && (File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath));
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                string newPath = GUILayout.TextField(m_enteredFilePath, style);
                if (m_enteredFilePath != newPath)
                {
                    m_enteredFilePath = newPath;
                    isDirty = true;
                }

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string path = SelectFile();
                    isDirty |= SetNewPath(path);
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_enteredFilePath);
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            Setup();
            
            bool exists = PathExists();
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            style.alignment = TextAnchor.MiddleLeft;
            
            string displayedPath = GetButtonText(maxWidth);
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = SelectFile();
                isDirty |= SetNewPath(newPath);
            }
        }

        protected abstract string SelectFile();

        private bool SetNewPath(string newPath)
        {
            if (newPath == m_enteredFilePath || string.IsNullOrEmpty(newPath))
            {
                return false;
            }
            
            m_enteredFilePath = newPath;
            return true;

        }

        private string GetButtonText(float maxWidth)
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
                displayedPath = ButtonText;
            }

            return displayedPath;
        }

        private bool PathExists()
        {
            if (string.IsNullOrEmpty(m_enteredFilePath))
            {
                return true;
            }
            
            return File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath);
        }

        public override async Task<bool> GetSource(BuildConfig buildConfig)
        {
            // Decide where we want to download to
            m_progressDescription = "Preparing...";
            m_downloadProgress = 0;
            string directoryPath = Utils.CacheFolder;
            if (!Directory.Exists(directoryPath))
            {
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

            string cacheFolderName = isDirectory ? new DirectoryInfo(sourcePath).Name : Path.GetFileNameWithoutExtension(sourcePath);
            string cacheFolderPath = Path.Combine(directoryPath, cacheFolderName + "_" + buildConfig.GUID);
            if (Directory.Exists(cacheFolderPath))
            {
                Debug.LogWarning($"Cached folder already exists: {cacheFolderPath}.\nLikely it wasn't cleaned up properly in an older build.\nDeleting now to avoid accidentally uploading the same build!");
                Directory.Delete(cacheFolderPath, true);
            }
            Directory.CreateDirectory(cacheFolderPath);
            
            if (isDirectory)
            {
                await Task.Run(async () =>
                {
                    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, cacheFolderPath));
                    }

                    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        await Utils.CopyFileAsync(newPath, newPath.Replace(sourcePath, cacheFolderPath));
                    }
                });
            }
            else
            {
                // Getting a file - put it in its own folder
                string path = Path.Combine(cacheFolderPath, Path.GetFileName(sourcePath));
                await Utils.CopyFileAsync(sourcePath, path);
            }

            m_finalSourcePath = cacheFolderPath;
            m_progressDescription = "Done!";
            return true;
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

        public override void CleanUp()
        {
            base.CleanUp();

            if (m_finalSourcePath == m_enteredFilePath)
            {
                return;
            }

            if (Utils.IsPathADirectory(m_finalSourcePath))
            {
                if (Directory.Exists(m_finalSourcePath))
                {
                    Debug.Log("Deleting cached file: " + m_finalSourcePath);
                    Directory.Delete(m_finalSourcePath, true);
                }
            }
            else
            {
                if (File.Exists(m_finalSourcePath))
                {
                    Debug.Log("Deleting cached file: " + m_finalSourcePath);
                    File.Delete(m_finalSourcePath);
                }
            }
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