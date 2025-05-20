using System.Collections.Generic;

namespace Wireframe
{
    public partial class BuildConfig
    {
        public bool Enabled { get; set; } = true;
        public string GUID { get; private set; }
        
        public List<SourceData> Sources => m_buildSources;
        public List<ModifierData > Modifiers => m_modifiers;
        public List<DestinationData> Destinations => m_buildDestinations;

        private List<SourceData> m_buildSources;
        private List<ModifierData> m_modifiers;
        private List<DestinationData> m_buildDestinations;

        public BuildConfig(string guid)
        {
            GUID = guid;
            m_buildSources = new List<SourceData>();
            m_modifiers = new List<ModifierData>();
            m_buildDestinations = new List<DestinationData>();
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
                    sourceData.Source.TryGetErrors(errors);
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

        public List<string> GetDestinationErrors()
        {
            List<string> errors = new List<string>();
            foreach (DestinationData destinationData in m_buildDestinations)
            {
                if(destinationData.Enabled && destinationData.Destination != null)
                {
                    destinationData.Destination.TryGetErrors(errors);
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
                    destinationData.Destination.TryGetWarnings(warnings);
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
                source.Source.TryGetErrors(errors);
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
            
            for (var i = 0; i < m_modifiers.Count; i++)
            {
                var source = m_modifiers[i];
                if (!source.Enabled)
                {
                    continue;
                }
                
                if (source.Modifier == null)
                {
                    reason = $"Modifier #{i+1} is not setup";
                    return false;
                }

                if (!source.Modifier.IsSetup(out string sourceReason))
                {
                    reason = $"Modifier #{i+1}: " + sourceReason;
                    return false;
                }
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
                destination.Destination.TryGetErrors(errors);
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

            reason = "";
            return true;
        }

        public bool IsBuilding()
        {
            foreach (SourceData source in m_buildSources)
            {
                if (source.Source == null)
                {
                    return false;
                }
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                if (destination.Destination == null)
                {
                    return false;
                }
            }
            
            foreach (SourceData source in m_buildSources)
            {
                if (source.Source.IsRunning)
                {
                    return true;
                }
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                if (destination.Destination.IsRunning)
                {
                    return true;
                }
            }

            return false;
        }

        public void CleanUp(BuildTaskReport.StepResult result)
        {
            foreach (SourceData source in m_buildSources)
            {
                source.Source?.CleanUp(result);
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                destination.Destination?.CleanUp(result);
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
    }
}