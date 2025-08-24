using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadAction : DropdownElement
    {
        public int Id { get; set; }
        
        protected bool m_actionInProgress;
        
        public virtual Task<bool> Prepare(bool successful, string buildDescription, UploadTaskReport.StepResult result)
        {
            return Task.FromResult(true);
        }
        
        public abstract Task<bool> Execute(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
        public virtual void CleanUp(UploadTaskReport.StepResult result)
        {
            m_actionInProgress = false;
        }

        public virtual void TryGetWarnings(List<string> warnings, StringFormatter.Context ctx)
        {
            
        }

        public virtual void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            
        }

        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}