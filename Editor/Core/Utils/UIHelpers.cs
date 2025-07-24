using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wireframe
{
    public static class UIHelpers
    {
        public class BuildSourcesPopup : CustomDropdown<BuildSourcesPopup.SourceData>
        {
            public override string FirstEntryText => "Choose Source";
            public class SourceData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }
            
            protected override List<SourceData> FetchAllData()
            {
                List<SourceData> sources = new List<SourceData>();
                foreach (Type type in InternalUtils.AllBuildSources())
                {
                    sources.Add(new SourceData()
                    {
                        Id = sources.Count + 1,
                        DisplayName = type.GetCustomAttribute<BuildSourceAttribute>()?.DisplayName ?? type.Name,
                        Type = type,
                    });
                }
                return sources;
            }

            public bool TryGetValueFromType(Type type, out SourceData data)
            {
                if (type != null)
                {
                    foreach (SourceData modifier in Values)
                    {
                        if (modifier.Type == type)
                        {
                            data = modifier;
                            return true;
                        }
                    }
                }

                data = null;
                return false;
            }
        }
        
        public class BuildModifiersPopup : CustomDropdown<BuildModifiersPopup.ModifierData>
        {
            public override string FirstEntryText => "Choose Modifier";
            public class ModifierData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }

            protected override List<ModifierData> FetchAllData()
            {
                List<ModifierData> modifiers = new List<ModifierData>();
                foreach (Type type in InternalUtils.AllBuildModifiers())
                {
                    modifiers.Add(new ModifierData()
                    {
                        Id = modifiers.Count + 1,
                        DisplayName = type.GetCustomAttribute<BuildModifierAttribute>()?.DisplayName ?? type.Name,
                        Type = type,
                    });
                }
                return modifiers;
            }
            
            public bool TryGetValueFromType(Type type, out ModifierData data)
            {
                if (type != null)
                {
                    foreach (ModifierData modifier in Values)
                    {
                        if (modifier.Type == type)
                        {
                            data = modifier;
                            return true;
                        }
                    }
                }

                data = null;
                return false;
            }
        }
        
        public class BuildDestinationsPopup : CustomDropdown<BuildDestinationsPopup.DestinationData>
        {
            public override string FirstEntryText => "Choose Destination";
            public class DestinationData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }

            protected override List<DestinationData> FetchAllData()
            {
                List<DestinationData> destinations = new List<DestinationData>();
                foreach (Type type in InternalUtils.AllBuildDestinations())
                {
                    destinations.Add(new DestinationData()
                    {
                        Id = destinations.Count + 1,
                        DisplayName = type.GetCustomAttribute<BuildDestinationAttribute>()?.DisplayName ?? type.Name,
                        Type = type,
                    });
                }
                return destinations;
            }

            public bool TryGetValueFromType(Type type, out DestinationData data)
            {
                if (type != null)
                {
                    foreach (DestinationData modifier in Values)
                    {
                        if (modifier.Type == type)
                        {
                            data = modifier;
                            return true;
                        }
                    }
                }

                data = null;
                return false;
            }
        }
        
        public class BuildActionPopup : CustomDropdown<BuildActionPopup.ActionData>
        {
            public override string FirstEntryText => "Choose Action";
            public class ActionData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }

            protected override List<ActionData> FetchAllData()
            {
                List<ActionData> actions = new List<ActionData>();
                foreach (Type type in InternalUtils.AllBuildActions())
                {
                    actions.Add(new ActionData()
                    {
                        Id = actions.Count + 1,
                        DisplayName = type.GetCustomAttribute<BuildActionAttribute>()?.DisplayName ?? type.Name,
                        Type = type,
                    });
                }
                return actions;
            }

            public bool TryGetValueFromType(Type type, out ActionData data)
            {
                if (type != null)
                {
                    foreach (ActionData action in Values)
                    {
                        if (action.Type == type)
                        {
                            data = action;
                            return true;
                        }
                    }
                }

                data = null;
                return false;
            }
        }

        public static BuildSourcesPopup SourcesPopup => m_sourcesPopup ?? (m_sourcesPopup = new BuildSourcesPopup());
        private static BuildSourcesPopup m_sourcesPopup;
        
        public static BuildModifiersPopup ModifiersPopup => m_modifiersPopup ?? (m_modifiersPopup = new BuildModifiersPopup());
        private static BuildModifiersPopup m_modifiersPopup;
        
        public static BuildDestinationsPopup DestinationsPopup => m_destinationsPopup ?? (m_destinationsPopup = new BuildDestinationsPopup());
        private static BuildDestinationsPopup m_destinationsPopup;
        
        public static BuildActionPopup ActionsPopup => m_actionsPopup ?? (m_actionsPopup = new BuildActionPopup());
        private static BuildActionPopup m_actionsPopup;

    }
}