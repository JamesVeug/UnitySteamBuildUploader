using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("DecompressModifier", "modifiers", "Decompresses a file and extract the files. eg: unzip a .zip file")]
    [UploadModifier("Decompress")]
    public partial class DecompressModifier : AUploadModifer
    {
        public enum DecompressionType
        {
            Zip,
        }
        
        [Wiki("Decompression Type", "Which format the file is compressed as.")]
        private DecompressionType m_decompressionType = DecompressionType.Zip;
        
        [Wiki("File Path", "The path to the compressed file. eg: Last Message 64-bit_Data/StreamingAssets/data.zip")]
        private string m_filePath = "";
        
        [Wiki("Target Path", "The path where the decompress files will go. If empty, the files will go to the root folder.")]
        private string m_targetPathToCompress = "";
        
        [Wiki("Remove Old file", "If true, the compressed file will be deleted after completion.")]
        private bool m_removeCompressedFile = true;
        
        public DecompressModifier()
        {
            // Required for reflection
        }
        
        public DecompressModifier(string fileName, string targetPath, DecompressionType decompressionType=DecompressionType.Zip, bool removeCompressedFile=true)
        {
            m_filePath = fileName;
            m_targetPathToCompress = targetPath;
            m_decompressionType = decompressionType;
            m_removeCompressedFile = removeCompressedFile;
        }

        public override void TryGetErrors(UploadConfig config, List<string> errors)
        {
            base.TryGetErrors(config, errors);
            if (string.IsNullOrEmpty(m_filePath))
            {
                errors.Add("No file path set to decompress");
            }
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedDirectory, UploadConfig uploadConfig,
            int buildIndex, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            string pathToFile = Path.Combine(cachedDirectory, StringFormatter.FormatString(m_filePath, ctx));
            string decompressDirectory = cachedDirectory;
            if (!string.IsNullOrEmpty(m_targetPathToCompress))
            {
                decompressDirectory = Path.Combine(cachedDirectory, StringFormatter.FormatString(m_targetPathToCompress, ctx));
            }
            

            bool successful = false;
            if (File.Exists(pathToFile))
            {
                // It's a file! (We only expect files)
                successful = await ZipUtils.UnZip(pathToFile, decompressDirectory, stepResult);
                if (successful)
                {
                    if (m_removeCompressedFile)
                    {
                        stepResult.AddLog("Deleting original file: " + pathToFile);
                        File.Delete(pathToFile);
                    }
                }
            }
            else
            {
                stepResult.SetFailed("Path to decompress does not exist: " + pathToFile);
            }

            
            return successful;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>()
            {
                { "filePath", m_filePath },
                { "targetPath", m_targetPathToCompress },
                { "removeCompressedFile", m_removeCompressedFile },
                { "decompressionType", m_decompressionType.ToString() }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("filePath", out object filePath))
            {
                m_filePath = filePath.ToString();
            }
            if (data.TryGetValue("targetPath", out object targetPath))
            {
                m_targetPathToCompress = targetPath.ToString();
            }
            if (data.TryGetValue("removeCompressedFile", out object removeCompressedFile))
            {
                m_removeCompressedFile = (bool)removeCompressedFile;
            }

            if (data.TryGetValue("decompressionType", out object decompressionType))
            {
                m_decompressionType = (DecompressionType)Enum.Parse(typeof(DecompressionType), decompressionType.ToString());
            }
        }
    }
}