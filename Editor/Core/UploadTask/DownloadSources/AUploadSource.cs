using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Sources are where files come from to be uploaded.
    /// Example: Let the user select a folder somewhere on their PC to be included and copied to the upload staging area.
    /// </summary>
    public abstract partial class AUploadSource : DropdownElement
    {
        public AUploadSource() : base()
        {
            // Required for reflection
        }

        /// <summary>
        /// Preparation step before any sources are started.
        /// If any return false then the upload is stopped.
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        /// <param name="token">Check this if the task has been told to stop mid-execution</param>
        /// <returns></returns>
        public virtual Task<bool> Prepare(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx, CancellationTokenSource token)
        {
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Retrieves all the content that is needed for this source to be included in the upload.
        /// When all sources have been retrieved SourceFilePath() will be invoked to get the content that will be copied.
        /// </summary>
        /// <param name="uploadConfig">Which config this source is owned by</param>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        /// <param name="token">Check this if the task has been told to stop mid-execution</param>
        /// <returns></returns>
        public abstract Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx, CancellationTokenSource token);
        
        /// <summary>
        /// Invoked after all sources are successful and will copy them to a cached folder location ready for upload.
        /// Return the path to the file or folder that is required to be included in your upload
        /// </summary>
        /// <returns>File or folder that is required</returns>
        public abstract string SourceFilePath();

        /// <summary>
        /// Executed at the end of the upload process regardless if it was successful or not.
        /// Reset any temporary data or delete files.
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <returns></returns>
        public virtual Task CleanUp(int configIndex, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check for anything that is concerning that the user should be warned about but not prevent upload.
        /// </summary>
        /// <param name="warnings">Add to this list any warnings you need</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        public virtual void TryGetWarnings(List<string> warnings)
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