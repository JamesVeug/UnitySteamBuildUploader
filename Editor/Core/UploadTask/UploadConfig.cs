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
        public List<PostUploadActionData> Actions => m_postActions;

        private List<SourceData> m_buildSources;
        private List<ModifierData> m_modifiers;
        private List<DestinationData> m_buildDestinations;
        private List<PostUploadActionData> m_postActions;
        
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
            m_postActions = new List<PostUploadActionData>();
            
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
            m_postActions.Clear();
            
            m_context = null;
        }
        
        public List<string> GetAllErrors()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceErrors());
            warnings.AddRange(GetModifierErrors());
            warnings.AddRange(GetDestinationErrors());
            warnings.AddRange(GetActionErrors());

            return warnings;
        }

        public List<string> GetAllWarnings()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceWarnings());
            warnings.AddRange(GetModifierWarnings());
            warnings.AddRange(GetDestinationWarnings());
            warnings.AddRange(GetActionWarnings());

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
                
                sourceData.Source.TryGetErrors(errors, m_context);
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
        
        public List<string> GetActionErrors()
        {
            List<string> errors = new List<string>();
            foreach (PostUploadActionData action in m_postActions)
            {
                if (action.WhenToExecute == PostUploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }
                
                if (action.ActionType == null)
                {
                    errors.Add("Action type not set");
                    continue;
                }
                
                action.UploadAction.TryGetErrors(errors, m_context);
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
        
        public List<string> GetActionWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (PostUploadActionData action in m_postActions)
            {
                if (action.WhenToExecute == PostUploadActionData.UploadCompleteStatus.Never || action.UploadAction == null)
                {
                    continue;
                }
                
                action.UploadAction.TryGetWarnings(warnings, m_context);
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
                
                destinationData.Destination.TryGetErrors(errors, m_context);
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

            for (int i = 0; i < m_postActions.Count; i++)
            {
                var action = m_postActions[i];
                if (action.WhenToExecute == PostUploadActionData.UploadCompleteStatus.Never)
                {
                    continue;
                }
                
                if (action.UploadAction == null)
                {
                    reason = $"Action #{i+1} is not setup";
                    return false;
                }

                List<string> errors = new List<string>();
                action.UploadAction.TryGetErrors(errors, m_context);
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
                    await source.Source.CleanUp(configIndex, result, buildConfig.Context);
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
        
        public void AddSource(AUploadSource source)
        {
            if (source == null)
            {
                return;
            }
            
            SourceData sourceData = new SourceData(source);
            m_buildSources.Add(sourceData);
        }
        
        public void AddDestination(DestinationData destination)
        {
            if (destination == null)
            {
                return;
            }
            
            m_buildDestinations.Add(destination);
        }
        
        public void AddDestination(AUploadDestination destination)
        {
            if (destination == null)
            {
                return;
            }
            
            DestinationData destinationData = new DestinationData(destination);
            m_buildDestinations.Add(destinationData);
        }
        
        public void AddAction(PostUploadActionData action)
        {
            if (action == null)
            {
                return;
            }
            
            m_postActions.Add(action);
        }
        
        public void AddAction(AUploadAction action)
        {
            if (action == null)
            {
                return;
            }
            
            PostUploadActionData actionData = new PostUploadActionData(action);
            m_postActions.Add(actionData);
        }
        
        public void AddModifier(ModifierData modifier)
        {
            if (modifier == null)
            {
                return;
            }
            
            m_modifiers.Add(modifier);
        }
        
        public void AddModifier(AUploadModifer modifier)
        {
            if (modifier == null)
            {
                return;
            }
            
            ModifierData modifierData = new ModifierData(modifier);
            m_modifiers.Add(modifierData);
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