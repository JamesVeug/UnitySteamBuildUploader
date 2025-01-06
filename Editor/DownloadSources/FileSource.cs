using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class FileSource : ASteamBuildSource
    {
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        
        private string m_finalSourcePath;
        private string m_enteredFilePath;

        public FileSource(SteamBuildWindow steamBuildWindow)
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
                GUILayout.Label("File Path:", GUILayout.Width(120));
                
                bool exists = !string.IsNullOrEmpty(m_enteredFilePath) && (File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath));
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                string newPath = GUILayout.TextField(m_enteredFilePath, style);

                if (GUILayout.Button("...", GUILayout.Width(50), GUILayout.MaxWidth(500)))
                {
                    newPath = EditorUtility.OpenFilePanel("Build Folder", "", "");
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_enteredFilePath);
                }

                if (newPath != m_enteredFilePath && !string.IsNullOrEmpty(newPath))
                {
                    m_enteredFilePath = newPath;
                    isDirty = true;
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty)
        {
            Setup();
            
            bool exists = FileExists();
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            if (GUILayout.Button(m_enteredFilePath, style))
            {
                string newPath = EditorUtility.OpenFilePanel("Select build to upload", "", "zip,exe");
                if (newPath != m_enteredFilePath && !string.IsNullOrEmpty(newPath))
                {
                    isDirty = true;
                    if (newPath.EndsWith(".exe"))
                    {
                        // Use path of exe instead
                        m_enteredFilePath = Path.GetDirectoryName(newPath);
                    }
                    else
                    {
                        m_enteredFilePath = newPath;
                    }
                }
            }
        }

        private bool FileExists()
        {
            if (string.IsNullOrEmpty(m_enteredFilePath))
            {
                return false;
            }
            
            return File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath);
        }

        public override async Task<bool> GetSource()
        {
            // Decide where we want to download to
            m_progressDescription = "Preparing...";
            m_downloadProgress = 0;
            string directoryPath = Application.persistentDataPath + "/ManualBuilds";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Make copy to avoid sharing conflicts
            string copyPath = directoryPath + "/" + Path.GetFileName(m_enteredFilePath);
            if (Directory.Exists(m_enteredFilePath))
            {
                // Given a directory that is not cached yet
                if (!Directory.Exists(copyPath))
                {
                    Directory.CreateDirectory(copyPath);
                    await Task.Run(() =>
                    {
                        foreach (string dirPath in Directory.GetDirectories(m_enteredFilePath, "*", SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(dirPath.Replace(m_enteredFilePath, copyPath));
                        }

                        foreach (string newPath in Directory.GetFiles(m_enteredFilePath, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(newPath, newPath.Replace(m_enteredFilePath, copyPath), true);
                        }
                    });
                }
            }
            else if (!File.Exists(copyPath))
            {
                // Given File that is not cached yet
                await CopyFileAsync(m_enteredFilePath, copyPath);
            }

            m_finalSourcePath = copyPath;
            m_progressDescription = "Done!";
            return true;
        }

        private static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
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
            return "Getting File";
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup(out string reason)
        {
            if (string.IsNullOrEmpty(m_enteredFilePath))
            {
                reason = "No file path set";
                return false;
            }

            if (!File.Exists(m_enteredFilePath) && !Directory.Exists(m_enteredFilePath))
            {
                reason = "File does not exist";
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
            
            if(File.Exists(m_finalSourcePath) && m_finalSourcePath != m_enteredFilePath)
            {
                Debug.Log("Deleting cached file: " + m_finalSourcePath);
                File.Delete(m_finalSourcePath);
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