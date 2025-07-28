using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AUploadModifer
    {
        internal UIHelpers.BuildDestinationsPopup.DestinationData ModifierType;
        
        public AUploadModifer()
        {
            // Required for reflection
        }

        public abstract Task<bool> ModifyBuildAtPath(string cachedDirectory, UploadConfig uploadConfig, int buildIndex,
            UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx);
        
        public virtual bool IgnoreFileDuringCacheSource(string path, int buildIndex, UploadTaskReport.StepResult stepResult)
        {
            return false;
        }
        
        public virtual void TryGetErrors(UploadConfig config, List<string> errors)
        {
            
        }
        
        public virtual void TryGetErrors(AUploadSource source, List<string> errors)
        {
            
        }
        
        public virtual void TryGetErrors(AUploadDestination destination, List<string> errors)
        {
            
        }

        public virtual void TryGetWarnings(List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(UploadConfig config, List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(AUploadSource source, List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(AUploadDestination destination, List<string> warnings)
        {
            
        }
        
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}