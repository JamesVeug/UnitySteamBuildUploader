using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Upload destinations are the end points where files are uploaded to.
    /// Example: Upload files to Steamworks or copy them to a folder on disk.
    /// </summary>
    public abstract partial class AUploadDestination
    {
        protected string m_cachedFolderPath;
        protected string m_buildDescription;

        public AUploadDestination()
        {
            // Required for reflection
        }

        /// <summary>
        /// Prepare the action to ensure it's ready to execute
        /// If this returns false then Execute is not... executed.
        /// </summary>
        /// <param name="taskGUID">Unique ID of the Task</param>
        /// <param name="configIndex">Index of the upload config that contains this destination</param>
        /// <param name="destinationIndex">Index of the destination in the config to upload</param>
        /// <param name="cachedFolderPath">The files we want to upload</param>
        /// <param name="buildDescription">Formatted description the user chose for the build</param>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        /// <returns>True if successfully prepared</returns>
        public virtual Task<bool> Prepare(string taskGUID, int configIndex, int destinationIndex, string cachedFolderPath,
            string buildDescription, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            m_cachedFolderPath = cachedFolderPath;
            m_buildDescription = buildDescription;
            stepResult.AddLog("No preparation needed for destination: " + DisplayName);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the upload to the destination
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        /// <returns>True if the upload was successful</returns>
        public abstract Task<bool> Upload(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
        /// <summary>
        /// Save your data as a Dictionary or List to persist it between sessions
        /// </summary>
        public abstract Dictionary<string, object> Serialize();
        
        /// <summary>
        /// Load your data from a Dictionary or List to restore it between sessions
        /// </summary>
        public abstract void Deserialize(Dictionary<string, object> data);

        /// <summary>
        /// Executed at the end of the upload process regardless if it was successful or not.
        /// Reset any temporary data or delete files.
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        public virtual Task CleanUp(UploadTaskReport.StepResult stepResult)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executed after all destinations are 
        /// </summary>
        /// <param name="stepResult"></param>
        /// <returns>True if successfully post uploaded</returns>
        public virtual Task<bool> PostUpload(UploadTaskReport.StepResult stepResult)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Check for anything that is concerning that the user should be warned about but not prevent upload.
        /// </summary>
        /// <param name="warnings">Add to this list any warnings you need</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        public virtual void TryGetWarnings(List<string> warnings, StringFormatter.Context ctx)
        {
            
        }

        /// <summary>
        /// Executed during GUI and before an upload starts to check for any warnings in the configuration of this source
        /// </summary>
        /// <param name="errors">Errors found in this method</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        public virtual void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            
        }
    }
}