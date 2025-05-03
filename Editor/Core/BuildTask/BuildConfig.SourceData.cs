using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfig
    {
        [Wiki("Sources", "Specify what data you want to include in your upload. All Sources are executed at the same time then copied to the cached folder 1 by 1.")]
        public class SourceData
        {
            [Wiki("Enabled", "When on, this source will gather the content from a source and get it ready to be uploaded")]
            public bool Enabled;
            
            [Wiki("Export Path", "A sub-path in the cached directory of which this source will be saved to before being modified and uploaded. Leave empty to save to the root folder.")]
            public string ExportFolder;
            
            [Wiki("Duplicate Files", "When copying files over and there already being the same file, what should we do with the new file?")]
            public Utils.FileExistHandling DuplicateFileHandling = Utils.FileExistHandling.Error;
            
            public ABuildSource Source;
            public UIHelpers.BuildSourcesPopup.SourceData SourceType;

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["enabled"] = Enabled,
                    ["duplicateFileHandling"] = DuplicateFileHandling.ToString(),
                    ["subFolderPath"] = ExportFolder,
                    ["sourceType"] = SourceType?.Type?.FullName,
                    ["source"] = Source?.Serialize()
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                Enabled = data.ContainsKey("enabled") ? (bool)data["enabled"] : true;
                DuplicateFileHandling = data.ContainsKey("duplicateFileHandling") ? (Utils.FileExistHandling)System.Enum.Parse(typeof(Utils.FileExistHandling), data["duplicateFileHandling"] as string) : Utils.FileExistHandling.Error;
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