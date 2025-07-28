using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadAction : DropdownElement
    {
        public bool IsRunning => m_actionInProgress;
        
        protected bool m_actionInProgress;
        protected bool m_successful;
        protected string m_buildDescription;
        
        public int Id { get; set; }
        
        public virtual Task<bool> Prepare(bool successful, string buildDescription, UploadTaskReport.StepResult result)
        {
            m_successful = successful;
            m_buildDescription = buildDescription;
            return Task.FromResult(true);
        }
        
        public abstract Task<bool> Execute(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
        public virtual void CleanUp(UploadTaskReport.StepResult result)
        {
            m_actionInProgress = false;
        }

        public virtual void TryGetWarnings(List<string> warnings)
        {
            
        }

        public virtual void TryGetErrors(List<string> errors)
        {
            
        }

        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}