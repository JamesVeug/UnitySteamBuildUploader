using System.Collections.Generic;
using System.Linq;

namespace Wireframe
{
    public partial class BuildConfig
    {
        public class SourceData
        {
            public bool Enabled;
            public ABuildSource Source;
            public string SubFolderPath;
            public UIHelpers.BuildSourcesPopup.SourceData SourceType;

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["enabled"] = Enabled,
                    ["subFolderPath"] = SubFolderPath,
                    ["sourceType"] = SourceType?.Type?.FullName,
                    ["source"] = Source?.Serialize()
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                Enabled = data.ContainsKey("enabled") ? (bool)data["enabled"] : true;
                SubFolderPath = data.ContainsKey("subFolderPath") ? data["subFolderPath"] as string : "";
                SourceType = new UIHelpers.BuildSourcesPopup.SourceData();
                if (data.TryGetValue("sourceType", out object sourceType) && sourceType != null)
                {
                    SourceType.Type = System.Type.GetType(sourceType as string);
                    if (SourceType.Type != null)
                    {
                        Source = Utils.CreateInstance<ABuildSource>(SourceType.Type);
                        if (Source != null)
                        {
                            Source.Deserialize(data["source"] as Dictionary<string, object>);
                        }
                    }
                }
            }
        }
    }
}