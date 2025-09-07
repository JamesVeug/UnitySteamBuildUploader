using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public partial class UploadConfig : StringFormatter.IContextModifier
    {
        public bool Enabled { get; set; } = true;
        public string GUID { get; private set; }
        public StringFormatter.Context Context => m_context;
        
        public List<SourceData> Sources => m_buildSources;
        public List<ModifierData > Modifiers => m_modifiers;
        public List<DestinationData> Destinations => m_buildDestinations;

        private List<SourceData> m_buildSources;
        private List<ModifierData> m_modifiers;
        private List<DestinationData> m_buildDestinations;
        
        private StringFormatter.Context m_context;

        public UploadConfig() : this("")
        {
            NewGUID();
        }

        public UploadConfig(string guid)
        {
            GUID = guid;
            
            m_buildSources = new List<SourceData>();
            m_modifiers = new List<ModifierData>();
            m_buildDestinations = new List<DestinationData>();
            
            m_context = new StringFormatter.Context();
            m_context.AddModifier(this);
        }

        public void NewGUID()
        {
            GUID = Guid.NewGuid().ToString().Substring(0, 5);
        }

        public void Clear()
        {
            m_buildSources.Clear();
            m_modifiers.Clear();
            m_buildDestinations.Clear();
            
            m_context = null;
        }
        
        public List<string> GetAllErrors()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceErrors());
            warnings.AddRange(GetDestinationErrors());

            return warnings;
        }

        public List<string> GetAllWarnings()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceWarnings());
            warnings.AddRange(GetDestinationWarnings());

            return warnings;
        }

        public List<string> GetSourceErrors()
        {
            List<string> errors = new List<string>();
            foreach (SourceData sourceData in m_buildSources)
            {
                if(sourceData.Enabled && sourceData.Source != null)
                {
                    sourceData.Source.TryGetErrors(errors, m_context);
                }
            }
            
            foreach (ModifierData modifer in m_modifiers)
            {
                if (!modifer.Enabled || modifer.ModifierType == null)
                {
                    continue;
                }
                
                foreach (SourceData sourceData in m_buildSources)
                {
                    modifer.Modifier?.TryGetErrors(sourceData.Source, errors);
                }
            }
            
            return errors;
        }
        
        public List<string> GetModifierErrors()
        {
            List<string> errors = new List<string>();
            foreach (ModifierData modifier in m_modifiers)
            {
                if (!modifier.Enabled || modifier.ModifierType == null)
                {
                    continue;
                }
                
                modifier.Modifier.TryGetErrors(this, errors);
            }
            
            return errors;
        }
        
        public List<string> GetModifierWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (ModifierData modifier in m_modifiers)
            {
                if (!modifier.Enabled || modifier.ModifierType == null)
                {
                    continue;
                }
                
                modifier.Modifier.TryGetWarnings(this, warnings);
            }
            
            return warnings;
        }

        public List<string> GetDestinationErrors()
        {
            List<string> errors = new List<string>();
            foreach (DestinationData destinationData in m_buildDestinations)
            {
                if(destinationData.Enabled && destinationData.Destination != null)
                {
                    destinationData.Destination.TryGetErrors(errors, m_context);
                }
            }
            
            foreach (ModifierData modifier in m_modifiers)
            {
                if (!modifier.Enabled || modifier.ModifierType == null)
                {
                    continue;
                }
                
                foreach (DestinationData destinationData in m_buildDestinations)
                {
                    modifier.Modifier.TryGetErrors(destinationData.Destination, errors);
                }
            }
            
            return errors;
        }

        public List<string> GetSourceWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (SourceData sourceData in m_buildSources)
            {
                if (sourceData.Enabled && sourceData.Source != null)
                {
                    sourceData.Source?.TryGetWarnings(warnings);
                }
            }
            
            foreach (ModifierData modifer in m_modifiers)
            {
                if (!modifer.Enabled || modifer.ModifierType == null)
                {
                    continue;
                }
                
                foreach (SourceData sourceData in m_buildSources)
                {
                    modifer.Modifier?.TryGetWarnings(sourceData.Source, warnings);
                }
            }
            
            return warnings;
        }

        public List<string> GetDestinationWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (DestinationData destinationData in m_buildDestinations)
            {
                if (destinationData.Enabled && destinationData.Destination != null)
                {
                    destinationData.Destination.TryGetWarnings(warnings, m_context);
                }
            }
            
            foreach (ModifierData modifier in m_modifiers)
            {
                if (!modifier.Enabled || modifier.ModifierType == null)
                {
                    continue;
                }
                
                foreach (DestinationData destinationData in m_buildDestinations)
                {
                    modifier.Modifier.TryGetWarnings(destinationData.Destination, warnings);
                }
            }
            
            return warnings;
        }

        public bool CanStartBuild(out string reason)
        {
            if(m_buildSources.Count == 0)
            {
                reason = "No Sources specified";
                return false;
            }
            
            bool atLeastOneSource = false;
            for (var i = 0; i < m_buildSources.Count; i++)
            {
                var source = m_buildSources[i];
                if (!source.Enabled)
                {
                    continue;
                }
                
                if (source.Source == null)
                {
                    reason = $"Source #{i+1} is not setup";
                    return false;
                }

                List<string> errors = new List<string>();
                source.Source.TryGetErrors(errors, m_context);
                if (errors.Count > 0)
                {
                    reason = $"Source #{i+1}: " + string.Join(", ", errors);
                    return false;
                }
                
                atLeastOneSource = true;
            }
            if (!atLeastOneSource)
            {
                reason = "Need at least 1 Source";
                return false;
            }

            bool atLeastOneDestination = false;
            for (var i = 0; i < m_buildDestinations.Count; i++)
            {
                var destination = m_buildDestinations[i];
                if (!destination.Enabled)
                {
                    continue;
                }
                
                if (destination.Destination == null)
                {
                    reason = $"Destination #{i+1} No destination specified.";
                    return false;
                }

                List<string> errors = new List<string>();
                destination.Destination.TryGetErrors(errors, m_context);
                if (errors.Count > 0)
                {
                    reason = $"Destination #{i+1}: " + string.Join(", ", errors);
                    return false;
                }
                
                atLeastOneDestination = true;
            }
            
            if (!atLeastOneDestination)
            {
                reason = "Need at least 1 Destination";
                return false;
            }
            
            for (var i = 0; i < m_modifiers.Count; i++)
            {
                var modifier = m_modifiers[i];
                if (!modifier.Enabled)
                {
                    continue;
                }
                
                if (modifier.Modifier == null)
                {
                    reason = $"Modifier #{i+1} is not setup";
                    return false;
                }

                List<string> errors = new List<string>();
                modifier.Modifier.TryGetErrors(this, errors);
                if (errors.Count > 0)
                {
                    reason = $"Modifier #{i+1}: " + string.Join(", ", errors);
                    return false;
                }
            }

            reason = "";
            return true;
        }

        public async Task CleanUp(int i, UploadConfig buildConfig, UploadTaskReport.StepResult result)
        {
            foreach (SourceData source in m_buildSources)
            {
                if (source.Enabled && source.Source != null)
                {
                    await source.Source.CleanUp(i, result, buildConfig.Context);
                }
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                if (destination.Enabled && destination.Destination != null)
                {
                    await destination.Destination.CleanUp(result);
                }
            }
        }

        public void AddSource(SourceData source)
        {
            if (source == null)
            {
                return;
            }
            
            m_buildSources.Add(source);
        }
        
        public void AddDestination(DestinationData destination)
        {
            if (destination == null)
            {
                return;
            }
            
            m_buildDestinations.Add(destination);
        }
        
        public void AddModifier(ModifierData modifier)
        {
            if (modifier == null)
            {
                return;
            }
            
            m_modifiers.Add(modifier);
        }

        public bool ReplaceString(string key, out string value)
        {
            foreach (SourceData source in Sources)
            {
                if (!source.Enabled)
                {
                    continue;
                }
                        
                if (source.Source is StringFormatter.IContextModifier modifier)
                {
                    if (modifier.ReplaceString(key, out value))
                    {
                        return true;
                    }
                }
            }

            value = "";
            return false;
        }
    }
}