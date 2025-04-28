using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfig
    {
        [Wiki("Modifiers", "Make changes to the files gathered from sources before uploading. Modifiers are executed in one at a time and in order.")]
        public class ModifierData
        {
            [Wiki("Enabled", "When on, this modifier will modify the content of the files for a build.")]
            public bool Enabled;
            public ABuildConfigModifer Modifier;
            public UIHelpers.BuildModifiersPopup.ModifierData ModifierType;

            public ModifierData()
            {
                
            }

            public ModifierData(ABuildConfigModifer modifier, bool enabled)
            {
                Enabled = enabled;
                Modifier = modifier;
                ModifierType = UIHelpers.ModifiersPopup.Values.FirstOrDefault(a => a.Type == modifier.GetType());
            }

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["enabled"] = Enabled,
                    ["modifierType"] = ModifierType?.Type?.FullName,
                    ["modifier"] = Modifier?.Serialize()
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                Enabled = data.ContainsKey("enabled") ? (bool)data["enabled"] : true;
                ModifierType = new UIHelpers.BuildModifiersPopup.ModifierData();
                if (data.TryGetValue("modifierType", out object modifierType) && modifierType != null)
                {
                    Type type = Type.GetType(modifierType as string);
                    if (UIHelpers.ModifiersPopup.TryGetValueFromType(type, out ModifierType))
                    {
                        if (Utils.CreateInstance(ModifierType.Type, out Modifier))
                        {
                            Modifier.Deserialize(data["modifier"] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Modifier type {modifierType} not found");
                    }
                }
                else
                {
                    Debug.LogError($"Modifier type not found in data: {modifierType}");
                }
            }
        }
    }
}