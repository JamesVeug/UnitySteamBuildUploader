using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadDestination : DropdownElement
    {
        public int Id { get; set; }
        
        protected string m_filePath;
        protected string m_buildDescription;

        public AUploadDestination()
        {
            // Required for reflection
        }


        public virtual Task<bool> Prepare(string taskGUID, int configIndex, int destinationIndex, string filePath,
            string buildDescription, UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            m_filePath = filePath;
            m_buildDescription = buildDescription;
            result.AddLog("No preparation needed for destination: " + DisplayName);
            return Task.FromResult(true);
        }

        public abstract Task<bool> Upload(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> s);

        public virtual Task CleanUp(UploadTaskReport.StepResult result)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> PostUpload(UploadTaskReport.StepResult report)
        {
            return Task.FromResult(true);
        }

        public virtual void TryGetWarnings(List<string> warnings, StringFormatter.Context ctx)
        {
                
        }

        public virtual void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            
        }
    }
}