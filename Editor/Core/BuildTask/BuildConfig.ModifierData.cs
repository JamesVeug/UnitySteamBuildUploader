using System.Collections.Generic;
using System.Linq;

namespace Wireframe
{
    public partial class BuildConfig
    {
        public class ModifierData
        {
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
                    ModifierType.Type = System.Type.GetType(modifierType as string);
                    if (ModifierType.Type != null)
                    {
                        Modifier = Utils.CreateInstance<ABuildConfigModifer>(ModifierType.Type);
                        if (Modifier != null)
                        {
                            Modifier.Deserialize(data["modifier"] as Dictionary<string, object>);
                        }
                    }
                }
            }
        }
    }
}