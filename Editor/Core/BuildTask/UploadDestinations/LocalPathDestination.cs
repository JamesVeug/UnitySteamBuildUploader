using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Move the modified build to a local path on the users computer
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [BuildDestination("LocalPath")]
    public partial class LocalPathDestination : ABuildDestination
    {
        private string m_localPath = "";
        private string m_fileName = "";
        private bool m_zipContent = false;

        public LocalPathDestination() : base()
        {
            // Required for reflection
        }
        
        public LocalPathDestination(string localPath, string fileName, bool zipContent) : base()
        {
            m_localPath = localPath;
            m_fileName = fileName;
            m_zipContent = zipContent;
        }

        private string FullPath()
        {
            string path = m_localPath;
            if (m_zipContent)
            {
                path += m_fileName + ".zip";
            }
            
            return path;
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

        public override async Task<bool> Upload(BuildTaskReport.StepResult result)
        {
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
                if (!await ZipUtils.Zip(m_filePath, fullPath, result))
                {
                    return false;
                }
            }
            else if (Utils.IsPathADirectory(m_filePath))
            {
                if (!await Utils.CopyDirectoryAsync(m_filePath, fullPath, result))
                {
                    return false;
                }
            }
            else
            {
                await Utils.CopyFileAsync(m_filePath, fullPath);
            }
            
            m_uploadProgress = 1;
            return true;
        }

        public override Task<bool> PostUpload(BuildTaskReport.StepResult result)
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
            return Task.FromResult(true);
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

            if (m_zipContent)
            {
                if (string.IsNullOrEmpty(m_fileName))
                {
                    reason = "No Name selected";
                    return false;
                }
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