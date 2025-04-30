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

        public List<string> GetAllWarnings()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceWarnings());
            warnings.AddRange(GetDestinationWarnings());

            return warnings;
        }

        public List<string> GetSourceWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (ModifierData modifer in m_modifiers)
            {
                if (modifer.ModifierType == null || !modifer.Enabled)
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
            foreach (ModifierData modifier in m_modifiers)
            {
                if (modifier.ModifierType == null || !modifier.Enabled)
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

                if (!source.Source.IsSetup(out string sourceReason))
                {
                    reason = $"Source #{i+1}: " + sourceReason;
                    return false;
                }
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

            if (m_buildDestinations.Count == 0)
            {
                reason = "No Destination specified";
                return false;
            }
            
            for (var i = 0; i < m_buildDestinations.Count; i++)
            {
                var destination = m_buildDestinations[i];
                if (!destination.Enabled)
                {
                    continue;
                }
                
                if (destination.Destination == null)
                {
                    reason = $"Destination #{i+1} is not setup";
                    return false;
                }

                if (!destination.Destination.IsSetup(out string destinationReason))
                {
                    reason = $"Destination #{i+1}: " + destinationReason;
                    return false;
                }
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