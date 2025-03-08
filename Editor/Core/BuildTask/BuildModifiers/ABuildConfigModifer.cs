using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    internal abstract class ABuildConfigModifer
    {
        public abstract bool IsSetup(out string reason);
        public abstract void Initialize(Action onChanged);
        public abstract Task<UploadResult> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex);
        
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