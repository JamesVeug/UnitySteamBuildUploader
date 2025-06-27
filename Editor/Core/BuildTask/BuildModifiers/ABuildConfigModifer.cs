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

        public abstract Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex, BuildTaskReport.StepResult stepResult);
        
        public virtual void TryGetErrors(BuildConfig config, List<string> errors)
        {
            foreach (BuildConfig.SourceData data in config.Sources)
            {
                if(!data.Enabled || data.Source == null)
                {
                    continue;
                }
                
                TryGetErrors(data.Source, errors);
            }
            
            foreach (BuildConfig.DestinationData destination in config.Destinations)
            {
                if(!destination.Enabled || destination.Destination == null)
                {
                    continue;
                }
                
                TryGetErrors(destination.Destination, errors);
            }
        }
        
        public virtual void TryGetErrors(ABuildSource source, List<string> errors)
        {
            
        }
        
        public virtual void TryGetErrors(ABuildDestination destination, List<string> errors)
        {
            
        }

        public virtual void TryGetWarnings(List<string> warnings)
        {
            
        }
        
        public virtual void TryGetWarnings(BuildConfig config, List<string> warnings)
        {
            foreach (BuildConfig.SourceData data in config.Sources)
            {
                if(!data.Enabled || data.Source == null)
                {
                    continue;
                }
                
                TryGetWarnings(data.Source, warnings);
            }
            
            foreach (BuildConfig.DestinationData destination in config.Destinations)
            {
                if(!destination.Enabled || destination.Destination == null)
                {
                    continue;
                }
                
                TryGetWarnings(destination.Destination, warnings);
            }
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