using System.Collections.Generic;

namespace Wireframe
{
    public partial class BuildConfig
    {
        public class DestinationData
        {
            public bool Enabled;
            public ABuildDestination Destination;
            public UIHelpers.BuildDestinationsPopup.DestinationData DestinationType;

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["enabled"] = Enabled,
                    ["destinationType"] = DestinationType?.Type?.FullName,
                    ["destination"] = Destination?.Serialize(),
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                Enabled = data.ContainsKey("enabled") ? (bool)data["enabled"] : true;
                DestinationType = new UIHelpers.BuildDestinationsPopup.DestinationData();
                if (data.TryGetValue("destinationType", out object destinationType) && destinationType != null)
                {
                    DestinationType.Type = System.Type.GetType(destinationType as string);
                    if (DestinationType.Type != null)
                    {
                        Destination = System.Activator.CreateInstance(DestinationType.Type, new object[] { null }) as ABuildDestination;
                        if (Destination != null)
                        {
                            Destination.Deserialize(data["destination"] as Dictionary<string, object>);
                        }
                    }
                }
            }
        }
    }
}