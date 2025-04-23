using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Move the modified build to a local path on the users computer
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    public class LocalPathDestination : ABuildDestination
    {
        public override string DisplayName => "LocalPath";
        private string ButtonText => "Choose Local Path...";
        
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        private string m_localPath = "";
        private string m_fileName = "";
        private bool m_zipContent = false;

        public LocalPathDestination() : base(null)
        {
            
        }
        
        public LocalPathDestination(string localPath, string fileName, bool zipContent) : base(null)
        {
            m_localPath = localPath;
            m_fileName = fileName;
            m_zipContent = zipContent;
        }

        internal LocalPathDestination(BuildUploaderWindow window) : base(window)
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
                GUILayout.Label("Directory:", GUILayout.Width(120));

                bool exists = !string.IsNullOrEmpty(m_localPath) && Directory.Exists(m_localPath);
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                string newPath = GUILayout.TextField(m_localPath, style);
                if (m_localPath != newPath)
                {
                    m_localPath = newPath;
                    isDirty = true;
                }

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string path = SelectFile();
                    isDirty |= SetNewPath(path);
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_localPath);
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Name (No extension):", GUILayout.Width(120));
                string newPath = GUILayout.TextField(m_fileName);
                if (m_fileName != newPath)
                {
                    m_fileName = newPath;
                    isDirty = true;
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));
                
                bool newZip = EditorGUILayout.Toggle(m_zipContent);
                if (m_zipContent != newZip)
                {
                    m_zipContent = newZip;
                    isDirty = true;
                }
            }
            
            GUILayout.Label("Full Path: " + FullPath());
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

        private string FullPath()
        {
            string path = m_localPath;
            string name = m_fileName;
            if (!string.IsNullOrEmpty(name))
            {
                path = Path.Combine(path, name);
            }
            
            if (m_zipContent)
            {
                path += ".zip";
            }
            
            return path;
        }

        private string GetButtonText(float maxWidth)
        {
            string fullPath = FullPath();
            string displayedPath = fullPath;
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
                
                if(displayedPath.Length < fullPath.Length)
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

        protected virtual string SelectFile()
        {
            return EditorUtility.OpenFolderPanel("Select Folder to upload", m_localPath, "");
        }
        
        private bool PathExists()
        {
            if (string.IsNullOrEmpty(m_localPath))
            {
                return true;
            }
            
            return File.Exists(m_localPath) || Directory.Exists(m_localPath);
        }

        private bool SetNewPath(string newPath)
        {
            if (newPath == m_localPath || string.IsNullOrEmpty(newPath))
            {
                return false;
            }
            
            m_localPath = newPath;
            return true;
        }

        public override async Task<bool> Upload(string filePath, string buildDescription, BuildTaskReport.StepResult result)
        {
            m_uploadInProgress = true;
            m_uploadProgress = 1;
            
            string fullPath = FullPath();
            string directory = m_zipContent ? Path.GetDirectoryName(fullPath) : fullPath;

            // Delete existing content
            if (Directory.Exists(fullPath))
            {
                result.AddLog($"Deleting existing directory: {fullPath}");
                Directory.Delete(fullPath, true);
            }
            else if (File.Exists(fullPath))
            {
                result.AddLog($"Deleting existing file: {fullPath}");
                File.Delete(fullPath);
            }
            
            // Create directory
            if (!Directory.Exists(directory))
            {
                result.AddLog($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }
            
            // Copy contents
            if (m_zipContent)
            {
                if (!await ZipUtils.Zip(filePath, fullPath, result))
                {
                    return false;
                }
            }
            else if (Utils.IsPathADirectory(filePath))
            {
                if (!await Utils.CopyDirectoryAsync(filePath, fullPath, result))
                {
                    return false;
                }
            }
            else
            {
                await Utils.CopyFileAsync(filePath, fullPath);
            }
            
            return true;
        }

        public override void PostUpload(BuildTaskReport.StepResult result)
        {
            string fullPath = FullPath();
            if (Path.HasExtension(fullPath))
            {
                fullPath = Path.GetDirectoryName(fullPath);
            }
            
            List<string> allFiles = Utils.GetSortedFilesAndDirectories(fullPath);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"LocalPathDestination: " + fullPath);
            foreach (string file in allFiles)
            {
                sb.AppendLine("\t-" + file);
            }
            result.AddLog(sb.ToString());
        }

        public override string ProgressTitle()
        {
            return "Copying to Local Path";
        }

        public override bool IsSetup(out string reason)
        {
            if (string.IsNullOrEmpty(m_localPath))
            {
                reason = "No local path selected";
                return false;
            }
            
            if (string.IsNullOrEmpty(m_fileName))
            {
                reason = "No Name selected";
                return false;
            }
            
            reason = "";
            return true;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["m_localPath"] = m_localPath;
            data["m_fileName"] = m_fileName;
            data["m_zipContent"] = m_zipContent;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_localPath = (string)data["m_localPath"];
            m_fileName = (string)data["m_fileName"];
            m_zipContent = (bool)data["m_zipContent"];
        }
    }
}