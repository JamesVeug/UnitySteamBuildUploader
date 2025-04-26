using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    [BuildModifier("Compress")]
    public partial class CompressModifier : ABuildConfigModifer
    {
        public enum CompressionType
        {
            Zip,
        }
        
        private bool m_removeContentAfterCompress = true;
        private string m_compressedFileName = "";
        private string m_subPathToCompress = "";
        private CompressionType m_compressionType = CompressionType.Zip;
        
        public CompressModifier()
        {
            // Required for reflection
        }
        
        public override bool IsSetup(out string reason)
        {
            if (string.IsNullOrEmpty(m_compressedFileName))
            {
                reason = "No compressed file name set";
                return false;
            }

            reason = "";
            return true;
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex, BuildTaskReport.StepResult stepResult)
        {
            string pathToCompress = cachedDirectory;
            if (!string.IsNullOrEmpty(m_subPathToCompress))
            {
                pathToCompress = Path.Combine(cachedDirectory, m_subPathToCompress);
            }
            
            string compressedFileName = m_compressedFileName;
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
                { "subPathToCompress", m_subPathToCompress },
                { "compressionType", m_compressionType.ToString() }
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
                m_subPathToCompress = data["subPathToCompress"].ToString();
            }

            if (data.ContainsKey("compressionType"))
            {
                m_compressionType = (CompressionType)System.Enum.Parse(typeof(CompressionType), data["compressionType"].ToString());
            }
        }
    }
}