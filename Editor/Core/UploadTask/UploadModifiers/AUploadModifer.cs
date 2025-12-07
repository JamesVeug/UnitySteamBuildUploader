using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Modifies the caches files from the sources before they are sent to the destinations.
    /// Example: Zip files, Encrypt files, Change file names
    /// </summary>
    public abstract partial class AUploadModifer
    {
        public Context Context => m_context;
        protected Context m_context;
        
        public AUploadModifer()
        {
            m_context = CreateContext();
        }


        protected virtual Context CreateContext()
        {
            return new Context();
        }

        /// <summary>
        /// Executed to modify the files in the cached directory before they are uploaded.
        /// </summary>
        /// <param name="cachedFolderPath">The files we want to upload</param>
        /// <param name="uploadConfig">The config that contains this modifier</param>
        /// <param name="configIndex">Index of the config that contains this modifier</param>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <returns></returns>
        public abstract Task<bool> ModifyBuildAtPath(string cachedFolderPath, UploadConfig uploadConfig,
            int configIndex,
            UploadTaskReport.StepResult stepResult);
        
        /// <summary>
        /// Executed per file in the "cache source files" step to determine if the file should be ignored and not copied to the cache.
        /// </summary>
        /// <param name="filePath">Path of the file being considered to be copied over to the cache</param>
        /// <param name="configIndex">Index of the config that contains this modifier</param>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <returns>True if the file at filePath should NOT be copied over to the cache before uploading</returns>
        public virtual bool IgnoreFileDuringCacheSource(string filePath, int configIndex, UploadTaskReport.StepResult stepResult)
        {
            return false;
        }
        
        /// <summary>
        /// Get errors that would prevent this modifier from working
        /// Example: A service is not enabled ot a field is not set
        /// </summary>
        /// <param name="config">Config that we are checking for errors</param>
        /// <param name="errors">Add errors to this to prevent the user from uploading</param>
        public virtual void TryGetErrors(UploadConfig config, List<string> errors)
        {
            
        }
        
        /// <summary>
        /// Get errors according to the provided source that would prevent this modifier from working
        /// Example: Check for a specific source type that this modifier doesn't want you using
        /// </summary>
        /// <param name="source">The source to check for errors</param>
        /// <param name="errors">Add errors to this to prevent the user from uploading</param>
        public virtual void TryGetErrors(AUploadSource source, List<string> errors)
        {
            
        }
        
        /// <summary>
        /// Get errors according to the provided destination that would prevent this modifier from working
        /// Example: Check for a specific destination type that this modifier doesn't want you using
        /// </summary>
        /// <param name="destination">The destination to check for errors</param>
        /// <param name="errors">Add errors to this to prevent the user from uploading</param>
        public virtual void TryGetErrors(AUploadDestination destination, List<string> errors)
        {
            
        }
        
        /// <summary>
        /// Get warnings to alert the user about this modifier that won't prevent uploading
        /// Example: A path does not exist but we'll still create it
        /// </summary>
        /// <param name="config">The config to check for potential warnings</param>
        /// <param name="warnings">Add warnings to this to alert the user</param>
        public virtual void TryGetWarnings(UploadConfig config, List<string> warnings)
        {
            
        }
        
        /// <summary>
        /// Get warnings according to the provided source about potential issues that won't prevent uploading
        /// Example: This source type is not recommended with this modifier
        /// </summary>
        /// <param name="source">The source to check warnings</param>
        /// <param name="warnings">Add warnings to this to alert the user</param>
        public virtual void TryGetWarnings(AUploadSource source, List<string> warnings)
        {
            
        }
        
        /// <summary>
        /// Get warnings according to the provided destination about potential issues that won't prevent uploading
        /// Example: This destination type is not recommended with this modifier
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="warnings">Add warnings to this to alert the user</param>
        public virtual void TryGetWarnings(AUploadDestination destination, List<string> warnings)
        {
            
        }
        
        /// <summary>
        /// Save your data as a Dictionary or List to persist it between sessions
        /// </summary>
        public abstract Dictionary<string, object> Serialize();
        
        /// <summary>
        /// Load your data from a Dictionary or List to restore it between sessions
        /// </summary>
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}