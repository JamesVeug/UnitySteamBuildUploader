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
    [Wiki("LocalPathDestination", "destinations", "Copy the build to a location on your local pc.")]
    [BuildDestination("Local Path")]
    public partial class LocalPathDestination : ABuildDestination
    {
        [Wiki("Directory", "The absolute path of the folder to copy the files to. eg: C:/MyBuilds/TodaysBuild" +
                           "\n\nSee docs for formatting options like {version} and {time} to use in the path.")]
        private string m_localPath = "";
        
        [Wiki("Duplicate Files", "When copying files over and there already being the same file, what should we do with the new file?")]
        private Utils.FileExistHandling m_duplicateFileHandling = Utils.FileExistHandling.Overwrite;
        
        [Wiki("Zip Content", "If true, the content will be zipped into a single file.")]
        private bool m_zipContent = false;
        
        [Wiki("Name", "If Zip Content is true, This is the name of the zipped file.")]
        private string m_zippedFilesName = "";

        public LocalPathDestination() : base()
        {
            // Required for reflection
        }
        
        public LocalPathDestination(string localPath, string fileName, bool zipContent) : base()
        {
            m_localPath = localPath;
            m_zippedFilesName = fileName;
            m_zipContent = zipContent;
        }

        private string FullPath()
        {
            string path = StringFormatter.FormatString(m_localPath);
            if (m_zipContent)
            {
                path += StringFormatter.FormatString(m_zippedFilesName) + ".zip";
            }
            
            return path;
        }
        
        private bool PathExists()
        {
            if (string.IsNullOrEmpty(m_localPath))
            {
                return true;
            }
            
            string fullPath = FullPath();
            return File.Exists(fullPath) || Directory.Exists(fullPath);
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
                if (!await Utils.CopyDirectoryAsync(m_filePath, fullPath, m_duplicateFileHandling, result))
                {
                    return false;
                }
            }
            else
            {
                if (!await Utils.CopyFileAsync(m_filePath, fullPath, m_duplicateFileHandling, result))
                {
                    return false;
                }
            }
            
            m_uploadProgress = 1;
            return true;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (string.IsNullOrEmpty(m_localPath))
            {
                errors.Add("No local path selected");
            }

            if (m_zipContent)
            {
                if (string.IsNullOrEmpty(m_zippedFilesName))
                {
                    errors.Add("No Zipped Name specified");
                }
            }
        }

        public override void TryGetWarnings(List<string> warnings)
        {
            base.TryGetWarnings(warnings);

            if (!PathExists())
            {
                warnings.Add("Path does not exist but may be created during upload.");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["m_localPath"] = m_localPath;
            data["m_fileName"] = m_zippedFilesName;
            data["m_zipContent"] = m_zipContent;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_localPath = (string)data["m_localPath"];
            m_zippedFilesName = (string)data["m_fileName"];
            m_zipContent = (bool)data["m_zipContent"];
        }
    }
}