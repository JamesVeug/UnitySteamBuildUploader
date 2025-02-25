using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    internal class SteamAppData
    {
        public List<SteamApp> Configs = new List<SteamApp>();

        public List<(SteamApp, List<SteamDepot>)> ConfigToDepots()
        {
            var dataConfigToDepotOptionValues = new List<(SteamApp, List<SteamDepot>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                SteamApp config = Configs[i];
                string[] names = new string[config.Depots.Count];
                List<SteamDepot> values = new List<SteamDepot>(config.Depots.Count);

                for (var j = 0; j < config.Depots.Count; j++)
                {
                    names[j] = config.Depots[j].Name;
                }

                Array.Sort(names);
                for (int j = 0; j < names.Length; j++)
                {
                    for (var k = 0; k < config.Depots.Count; k++)
                    {
                        string depotName = config.Depots[k].Name;
                        if (names[j] == depotName)
                        {
                            values.Add(config.Depots[k]);
                            break;
                        }
                    }
                }

                dataConfigToDepotOptionValues.Add((config, values));
            }

            return dataConfigToDepotOptionValues;
        }

        public List<(SteamApp, List<SteamBranch>)> ConfigToBranches()
        {
            var dataConfigToDepotOptionValues = new List<(SteamApp, List<SteamBranch>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                SteamApp config = Configs[i];
                dataConfigToDepotOptionValues.Add((config, config.ConfigBranches));
            }

            return dataConfigToDepotOptionValues;
        }
    }
}