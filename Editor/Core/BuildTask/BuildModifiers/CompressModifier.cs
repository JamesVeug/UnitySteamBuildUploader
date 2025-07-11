using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("CompressModifier", "modifiers", "Compresses a file or directory into a smaller file. eg: .zip a directory")]
    [BuildModifier("Compress")]
    public partial class CompressModifier : ABuildConfigModifer
    {
        public enum CompressionType
        {
            Zip,
        }
        
        [Wiki("Compression Type", "Which format to compress the content to.")]
        private CompressionType m_compressionType = CompressionType.Zip;
        
        [Wiki("Compressed Name", "The name of the compressed file. eg: MyCompressedFile.zip")]
        private string m_compressedFileName = "";
        
        [Wiki("Target Path", "The path to the file or directory in the cached directory to compress. If empty, the entire build will be compressed.")]
        private string m_targetPathToCompress = "";
        
        [Wiki("Remove Old files", "If true, the original file or directory will be deleted after compression.")]
        private bool m_removeContentAfterCompress = true;
        
        public CompressModifier()
        {
            // Required for reflection
        }
        
        public CompressModifier(string fileName, string targetPath, CompressionType compressionType=CompressionType.Zip, bool removeContentAfterCompress=true)
        {
            m_compressedFileName = fileName;
            m_targetPathToCompress = targetPath;
            m_compressionType = compressionType;
            m_removeContentAfterCompress = removeContentAfterCompress;
        }

        public override void TryGetErrors(BuildConfig config, List<string> errors)
        {
            base.TryGetErrors(config, errors);
            if (string.IsNullOrEmpty(m_compressedFileName))
            {
                errors.Add("No compressed file name set");
            }
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex, BuildTaskReport.StepResult stepResult)
        {
            string pathToCompress = cachedDirectory;
            if (!string.IsNullOrEmpty(m_targetPathToCompress))
            {
                pathToCompress = Path.Combine(cachedDirectory, StringFormatter.FormatString(m_targetPathToCompress));
            }
            
            string compressedFileName = StringFormatter.FormatString(m_compressedFileName);
            if (!compressedFileName.EndsWith(".zip"))
            {
                compressedFileName += ".zip";
            }


            bool successful = false;
            if (File.Exists(pathToCompress))
            {
                // It's a file!
                string zipPath = Path.Combine(Path.GetDirectoryName(pathToCompress), compressedFileName);
                successful = await ZipUtils.Zip(pathToCompress, zipPath, stepResult);
                if (successful)
                {
                    stepResult.AddLog("Compressed file: " + pathToCompress + " to " + zipPath);
                    if (m_removeContentAfterCompress)
                    {
                        stepResult.AddLog("Deleting original file: " + pathToCompress);
                        File.Delete(pathToCompress);
                    }
                }
            }
            else if (Directory.Exists(pathToCompress))
            {
                // It's a directory!
                string[] files = Directory.GetFiles(pathToCompress).Concat(Directory.GetDirectories(pathToCompress)).ToArray();
                
                string zipResultDirectory = pathToCompress == cachedDirectory ? cachedDirectory : Path.GetDirectoryName(pathToCompress);
                string zipPath = Path.Combine(zipResultDirectory, compressedFileName);
                successful = await ZipUtils.Zip(pathToCompress, zipPath, stepResult);
                if (successful)
                {
                    stepResult.AddLog("Compressed directory: " + pathToCompress + " to " + zipPath);
                    if (m_removeContentAfterCompress)
                    {
                        string[] filesAfterZip = Directory.GetFiles(pathToCompress).Concat(Directory.GetDirectories(pathToCompress)).ToArray();
                        if (filesAfterZip.Length == files.Length)
                        {
                            stepResult.AddLog("Deleting original directory: " + pathToCompress);
                            Directory.Delete(pathToCompress, true);
                        }
                        else
                        {
                            foreach (string file in files)
                            {
                                if (File.Exists(file))
                                {
                                    stepResult.AddLog("Deleting original file: " + file);
                                    File.Delete(file);
                                }
                                else if (Directory.Exists(file))
                                {
                                    stepResult.AddLog("Deleting original folder: " + file);
                                    Directory.Delete(file, true);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                stepResult.SetFailed("Path to compress does not exist: " + pathToCompress);
            }

            
            return successful;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>()
            {
                { "compressedFileName", m_compressedFileName },
                { "subPathToCompress", m_targetPathToCompress },
                { "compressionType", m_compressionType.ToString() },
                { "removeContentAfterCompress", m_removeContentAfterCompress },
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("compressedFileName"))
            {
                m_compressedFileName = data["compressedFileName"].ToString();
            }

            if (data.ContainsKey("subPathToCompress"))
            {
                m_targetPathToCompress = data["subPathToCompress"].ToString();
            }

            if (data.ContainsKey("compressionType"))
            {
                m_compressionType = (CompressionType)System.Enum.Parse(typeof(CompressionType), data["compressionType"].ToString());
            }
            
            if (data.ContainsKey("removeContentAfterCompress"))
            {
                m_removeContentAfterCompress = (bool)data["removeContentAfterCompress"];
            }
        }
    }
}