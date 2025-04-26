using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class ABuildConfigModifer
    {
        internal UIHelpers.BuildDestinationsPopup.DestinationData ModifierType;
        
        public ABuildConfigModifer()
        {
            // Required for reflection
        }

        public abstract bool IsSetup(out string reason);
        public abstract Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex, BuildTaskReport.StepResult stepResult);
        
        public virtual void TryGetWarnings(BuildConfig config, List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(ABuildSource source, List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(ABuildDestination destination, List<string> warnings)
        {
            
        }

        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}