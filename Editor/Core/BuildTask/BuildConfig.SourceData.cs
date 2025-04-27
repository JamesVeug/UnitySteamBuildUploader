using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfig
    {
        [Wiki("Sources", "")]
        public class SourceData
        {
            [Wiki("Enabled", "When on, this source will not be triggered when making a build.")]
            public bool Enabled;
            
            [Wiki("Export Path", "A sub-path in the cached directory of which this source will be saved to before being modified and uploaded. Leave empty to save to the root folder.")]
            public string ExportFolder;
            
            public ABuildSource Source;
            public UIHelpers.BuildSourcesPopup.SourceData SourceType;

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["enabled"] = Enabled,
                    ["subFolderPath"] = ExportFolder,
                    ["sourceType"] = SourceType?.Type?.FullName,
                    ["source"] = Source?.Serialize()
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                Enabled = data.ContainsKey("enabled") ? (bool)data["enabled"] : true;
                ExportFolder = data.ContainsKey("subFolderPath") ? data["subFolderPath"] as string : "";
                SourceType = new UIHelpers.BuildSourcesPopup.SourceData();
                if (data.TryGetValue("sourceType", out object sourceType) && sourceType != null)
                {
                    var type = System.Type.GetType(sourceType as string);
                    if (UIHelpers.SourcesPopup.TryGetValueFromType(type, out SourceType))
                    {
                        if (Utils.CreateInstance(SourceType.Type, out Source))
                        {
                            Source.Deserialize(data["source"] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Cannot find type {sourceType}");
                    }
                }
                else
                {
                    Debug.LogError($"Source type {sourceType} not found");
                }
            }
        }
    }
}