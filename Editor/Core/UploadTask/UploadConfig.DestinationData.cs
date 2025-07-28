using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public partial class UploadConfig
    {
        [Wiki("Destinations", "Specify where you want to upload your build. All destinations are executed at the same time.")]
        public class DestinationData
        {
            [Wiki("Enabled", "When on, this destination will upload all content of the source files.")]
            public bool Enabled = true;
            public AUploadDestination Destination;
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
                    var type = System.Type.GetType(destinationType as string);
                    if (UIHelpers.DestinationsPopup.TryGetValueFromType(type, out DestinationType))
                    {
                        if (Utils.CreateInstance(DestinationType.Type, out Destination))
                        {
                            Destination.Deserialize(data["destination"] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Destination type {destinationType} not found");
                    }
                }
            }
        }
    }
}