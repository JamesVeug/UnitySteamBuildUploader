using System;
using System.Collections.Generic;

namespace Wireframe
{
    internal static class UIHelpers
    {
        internal class BuildSourcesPopup : CustomDropdown<BuildSourcesPopup.SourceData>
        {
            public override string FirstEntryText => "Choose Source";
            public class SourceData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }
            
            public override List<SourceData> GetAllData()
            {
                List<SourceData> sources = new List<SourceData>();
                foreach (Type type in InternalUtils.AllBuildSources())
                {
                    var temp = Activator.CreateInstance(type, new object[]{null}) as ABuildSource; // Look away
                    sources.Add(new SourceData()
                    {
                        Id = sources.Count + 1,
                        DisplayName = temp.DisplayName,
                        Type = type,
                    });
                }
                return sources;
            }
        }
        
        internal class BuildDestinationsPopup : CustomDropdown<BuildDestinationsPopup.DestinationData>
        {
            public override string FirstEntryText => "Choose Destination";
            public class DestinationData : DropdownElement
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public Type Type { get; set; }
            }
            
            public override List<DestinationData> GetAllData()
            {
                List<DestinationData> destinations = new List<DestinationData>();
                foreach (Type type in InternalUtils.AllBuildDestinations())
                {
                    var temp = Activator.CreateInstance(type, new object[]{null}) as ABuildDestination; // Look away
                    destinations.Add(new DestinationData()
                    {
                        Id = destinations.Count + 1,
                        DisplayName = temp.DisplayName,
                        Type = type,
                    });
                }
                return destinations;
            }
        }

        public static BuildSourcesPopup SourcesPopup => m_sourcesPopup ?? (m_sourcesPopup = new BuildSourcesPopup());
        private static BuildSourcesPopup m_sourcesPopup;
        
        public static BuildDestinationsPopup DestinationsPopup => m_destinationsPopup ?? (m_destinationsPopup = new BuildDestinationsPopup());
        private static BuildDestinationsPopup m_destinationsPopup;

    }
}