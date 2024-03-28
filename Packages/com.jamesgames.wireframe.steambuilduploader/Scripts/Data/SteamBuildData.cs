using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class SteamBuildData
    {
        public List<SteamBuildConfig> Configs = new List<SteamBuildConfig>();

        public List<(SteamBuildConfig, List<SteamBuildDepot>)> ConfigToDepots()
        {
            var dataConfigToDepotOptionValues = new List<(SteamBuildConfig, List<SteamBuildDepot>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                SteamBuildConfig config = Configs[i];
                string[] names = new string[config.Depots.Count];
                List<SteamBuildDepot> values = new List<SteamBuildDepot>(config.Depots.Count);

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

        public List<(SteamBuildConfig, List<string>)> ConfigToBranches()
        {
            var dataConfigToDepotOptionValues = new List<(SteamBuildConfig, List<string>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                SteamBuildConfig config = Configs[i];
                string[] names = new string[config.Branches.Count];
                List<string> values = new List<string>(config.Branches.Count);

                for (var j = 0; j < config.Branches.Count; j++)
                {
                    names[j] = config.Branches[j];
                }

                Array.Sort(names);
                for (int j = 0; j < names.Length; j++)
                {
                    for (var k = 0; k < config.Branches.Count; k++)
                    {
                        string depotName = config.Branches[k];
                        if (names[j] == depotName)
                        {
                            values.Add(config.Branches[k]);
                            break;
                        }
                    }
                }

                dataConfigToDepotOptionValues.Add((config, values));
            }

            return dataConfigToDepotOptionValues;
        }
    }
}