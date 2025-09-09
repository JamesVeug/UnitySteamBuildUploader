using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadAction : DropdownElement
    {
        public int Id { get; set; }
        
        public virtual Task<bool> Prepare(UploadTaskReport.StepResult stepResult)
        {
            return Task.FromResult(true);
        }
        
        public abstract Task<bool> Execute(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
        public virtual void CleanUp(UploadTaskReport.StepResult result)
        public virtual Task CleanUp(UploadTaskReport.StepResult result)
        {
            return Task.CompletedTask;
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