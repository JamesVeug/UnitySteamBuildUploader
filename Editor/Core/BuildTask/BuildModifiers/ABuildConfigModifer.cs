using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract class ABuildConfigModifer
    {
        public abstract bool IsSetup(out string reason);
        internal abstract void Initialize(Action onChanged);
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
        
        public abstract bool OnGUI();
        public abstract Dictionary<string, object> Serialize();
        public abstract void Deserialize(Dictionary<string, object> data);
    }
}