using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadDestination : DropdownElement
    {
        protected string m_filePath;
        protected string m_buildDescription;
        
        protected float m_uploadProgress;
        protected bool m_uploadInProgress;

        public AUploadDestination()
        {
            // Required for reflection
        }

        public bool IsRunning => m_uploadInProgress;
        public int Id { get; set; }

        public virtual Task<bool> Prepare(string filePath, string buildDescription, UploadTaskReport.StepResult result)
        {
            m_filePath = filePath;
            m_buildDescription = buildDescription;
            m_uploadInProgress = true;
            return Task.FromResult(true);
        }

        public abstract Task<bool> Upload(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> s);

        public virtual void CleanUp(UploadTaskReport.StepResult result)
        {
            m_uploadProgress = 0.0f;
            m_uploadInProgress = false;
        }

        public virtual float UploadProgress()
        {
            return m_uploadProgress;
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