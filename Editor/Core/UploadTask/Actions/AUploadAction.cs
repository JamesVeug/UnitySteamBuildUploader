using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Actions are steps in the upload process that can perform any kind of action - typically that do not modify files.
    /// Example: Send message to discord
    /// </summary>
    public abstract partial class AUploadAction
    {
        /// <summary>
        /// Prepare the action to ensure it's ready to execute
        /// If this returns false then Execute is not... executed.
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <returns>True if successfully prepared</returns>
        public virtual Task<bool> Prepare(UploadTaskReport.StepResult stepResult)
        {
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Runs the action
        /// </summary>
        /// <param name="stepResult">Information in the current upload step. Add logs to this or stop with SetFailed</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        /// <returns>True if successfully prepared</returns>
        public abstract Task<bool> Execute(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
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