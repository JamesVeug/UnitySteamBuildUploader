﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Move the modified build to a local path on the users computer
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki("LocalPathDestination", "destinations", "Copy the build to a location on your local pc.")]
    [UploadDestination("Local Path")]
    public partial class LocalPathDestination : AUploadDestination
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
        
        public LocalPathDestination(string localPath, Utils.FileExistHandling duplicateFileHandling = Utils.FileExistHandling.Overwrite) : base()
        {
            m_localPath = localPath;
            m_duplicateFileHandling = duplicateFileHandling;
        }

        public void ZipContents(string zippedFileName)
        {
            m_zipContent = true;
            m_zippedFilesName = zippedFileName;
        }

        private string FullPath(StringFormatter.Context ctx)
        {
            string path = StringFormatter.FormatString(m_localPath, ctx);
            if (m_zipContent)
            {
                path += StringFormatter.FormatString(m_zippedFilesName, ctx) + ".zip";
            }
            
            return path;
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

        public override async Task<bool> Upload(UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            string fullPath = FullPath(ctx);
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
                result.AddLog($"Zipping context to: {fullPath}");
                if (!await ZipUtils.Zip(m_cachedFolderPath, fullPath, result))
                {
                    return false;
                }
            }
            else if (Utils.IsPathADirectory(m_cachedFolderPath))
            {
                result.AddLog($"Copying directory to: {fullPath}");
                if (!await Utils.CopyDirectoryAsync(m_cachedFolderPath, fullPath, m_duplicateFileHandling, result))
                {
                    return false;
                }
            }
            else
            {
                result.AddLog($"Copying file to: {fullPath}");
                if (!await Utils.CopyFileAsync(m_cachedFolderPath, fullPath, m_duplicateFileHandling, result))
                {
                    return false;
                }
            }
            
            return true;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (string.IsNullOrEmpty(m_localPath))
            {
                errors.Add("No local path selected");
            }
            else if (Utils.PathContainsInvalidCharacters(FullPath(ctx)))
            {
                errors.Add("Path contains invalid characters");
            }

            if (m_zipContent)
            {
                if (string.IsNullOrEmpty(m_zippedFilesName))
                {
                    errors.Add("No Zipped Name specified");
                }
            }
        }

        public override void TryGetWarnings(List<string> warnings, StringFormatter.Context ctx)
        {
            base.TryGetWarnings(warnings, ctx);

            if (!Utils.PathExists(FullPath(ctx)))
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
            data["m_duplicateFileHandling"] = (int)m_duplicateFileHandling;
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_localPath = (string)data["m_localPath"];
            m_zippedFilesName = (string)data["m_fileName"];
            m_zipContent = (bool)data["m_zipContent"];

            if (data.TryGetValue("m_duplicateFileHandling", out object handling) && handling is long)
            {
                m_duplicateFileHandling = (Utils.FileExistHandling)(long)handling;
            }
            else
            {
                m_duplicateFileHandling = Utils.FileExistHandling.Error;
            }
        }
    }
}