using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadSource : DropdownElement
    {
        public AUploadSource() : base()
        {
            // Required for reflection
        }

        public int Id { get; set; }
        
        public abstract Task<bool> GetSource(UploadConfig uploadConfig, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx, CancellationTokenSource token);
        
        public abstract string SourceFilePath();

        public virtual void CleanUp(int i, UploadTaskReport.StepResult result)
        {
            
        }

        public virtual void TryGetWarnings(List<string> warnings)
        {
            
        }

        public virtual void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            
        }


        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}