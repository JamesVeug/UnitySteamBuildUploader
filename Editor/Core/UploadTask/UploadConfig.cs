using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wireframe
{
    public partial class UploadConfig
    {
        public bool Enabled { get; set; } = true;
        public string GUID { get; private set; }
        public Context Context => m_context;
        
        public List<SourceData> Sources => m_buildSources;
        public List<ModifierData > Modifiers => m_modifiers;
        public List<DestinationData> Destinations => m_buildDestinations;
        public List<UploadActionData> PostActions => m_postActions;

        private List<SourceData> m_buildSources;
        private List<ModifierData> m_modifiers;
        private List<DestinationData> m_buildDestinations;
        private List<UploadActionData> m_postActions;
        
        private UploadConfigContext m_context;

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
            m_postActions = new List<UploadActionData>();
            
            m_context = new UploadConfigContext(this);
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
            m_postActions.Clear();
            
            m_context = null;
        }
        
        public List<string> GetAllErrors()
        {
            List<string> errors = new List<string>();
            errors.AddRange(GetSourceErrors());
            errors.AddRange(GetModifierErrors());
            errors.AddRange(GetDestinationErrors());
            errors.AddRange(GetPostActionErrors());

            return errors;
        }

        public List<string> GetAllWarnings()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceWarnings());
            warnings.AddRange(GetModifierWarnings());
            warnings.AddRange(GetDestinationWarnings());
            warnings.AddRange(GetPostActionWarnings());

            return warnings;
        }

        public List<string> GetSourceErrors()
        {
            List<string> errors = new List<string>();
            foreach (SourceData sourceData in m_buildSources)
            {
                if (!sourceData.Enabled)
                {
                    continue;
                }

                if (sourceData.Source == null)
                {
                    errors.Add("Source not set");
                    continue;
                }
                
                sourceData.Source.TryGetErrors(errors);
            }
            
            return errors;
        }
        
        public List<string> GetModifierErrors()
        {
            List<string> errors = new List<string>();
            foreach (ModifierData modifier in m_modifiers)
            {
                if (!modifier.Enabled)
                {
                    continue;
                }
                
                if (modifier.ModifierType == null)
                {
                    errors.Add("Modifier type not set");
                    continue;
                }
                
                modifier.Modifier.TryGetErrors(this, errors);
            }
            
            return errors;
        }
        
        public List<string> GetPostActionErrors()
        {
            List<string> errors = new List<string>();
            foreach (UploadActionData action in m_postActions)
            {
                if (action.WhenToExecute == UploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }
                
                if (action.ActionType == null)
                {
                    errors.Add("Action type not set");
                    continue;
                }
                
                action.UploadAction.TryGetErrors(errors);
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
        
        public List<string> GetPostActionWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (UploadActionData action in m_postActions)
            {
                if (action.WhenToExecute == UploadActionData.UploadCompleteStatus.Never || action.UploadAction == null)
                {
                    continue;
                }
                
                action.UploadAction.TryGetWarnings(warnings);
            }
            
            return warnings;
        }

        public List<string> GetDestinationErrors()
        {
            List<string> errors = new List<string>();
            foreach (DestinationData destinationData in m_buildDestinations)
            {
                if (!destinationData.Enabled)
                {
                    continue;
                }

                if (destinationData.Destination == null)
                {
                    errors.Add("Destination not set");
                    continue;
                }
                
                destinationData.Destination.TryGetErrors(errors);
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
                    sourceData.Source.TryGetWarnings(warnings);
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

            for (int i = 0; i < m_postActions.Count; i++)
            {
                var action = m_postActions[i];
                if (action.WhenToExecute == UploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }
                
                if (action.UploadAction == null)
                {
                    reason = $"Action #{i+1} is not setup";
                    return false;
                }

                List<string> errors = new List<string>();
                action.UploadAction.TryGetErrors(errors);
                if (errors.Count > 0)
                {
                    reason = $"Action #{i+1}: " + string.Join(", ", errors);
                    return false;
                }
            }

            reason = "";
            return true;
        }

        public async Task CleanUp(int configIndex, UploadConfig buildConfig, UploadTaskReport.StepResult result)
        {
            foreach (SourceData source in m_buildSources)
            {
                if (source.Enabled && source.Source != null)
                {
                    await source.Source.CleanUp(configIndex, result);
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

            if (source.Source != null)
            {
                source.Source.Context.SetParent(m_context);
            }
            
            m_buildSources.Add(source);
        }
        
        public void AddSource(AUploadSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Context.SetParent(m_context);
            
            SourceData sourceData = new SourceData(source);
            m_buildSources.Add(sourceData);
        }
        
        public void AddDestination(DestinationData destination)
        {
            if (destination == null)
            {
                return;
            }

            if (destination.Destination != null)
            {
                destination.Destination.Context.SetParent(m_context);
            }
            
            m_buildDestinations.Add(destination);
        }
        
        public void AddDestination(AUploadDestination destination)
        {
            if (destination == null)
            {
                return;
            }
            
            destination.Context.SetParent(m_context);
            
            DestinationData destinationData = new DestinationData(destination);
            m_buildDestinations.Add(destinationData);
        }
        
        public void AddPostAction(UploadActionData action)
        {
            if (action == null)
            {
                return;
            }

            if (action.UploadAction != null)
            {
                action.UploadAction.Context.SetParent(m_context);
            }

            m_postActions.Add(action);
        }
        
        public void AddPostAction(AUploadAction action, UploadActionData.UploadCompleteStatus completeStatus = UploadActionData.UploadCompleteStatus.Always)
        {
            if (action == null)
            {
                return;
            }
            
            action.Context.SetParent(m_context);
            
            UploadActionData actionData = new UploadActionData(action, completeStatus);
            m_postActions.Add(actionData);
        }
        
        public void AddModifier(ModifierData modifier)
        {
            if (modifier == null)
            {
                return;
            }

            if (modifier.Modifier != null)
            {
                modifier.Modifier.Context.SetParent(m_context);
            }

            m_modifiers.Add(modifier);
        }
        
        public void AddModifier(AUploadModifer modifier)
        {
            if (modifier == null)
            {
                return;
            }
            
            modifier.Context.SetParent(m_context);
            
            ModifierData modifierData = new ModifierData(modifier);
            m_modifiers.Add(modifierData);
        }
    }
}