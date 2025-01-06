using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static class SteamBuildWindowUtil
    {
        private static readonly string FilePath = Application.dataPath + "/../SteamBuilder/SteamWorksConfig.json";

        internal class SteamDepotPopup : CustomMultiDropdown<SteamBuildConfig, SteamBuildDepot>
        {
            public override List<(SteamBuildConfig, List<SteamBuildDepot>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToDepots();
            }
        }

        internal class SteamConfigPopup : CustomDropdown<SteamBuildConfig>
        {
            public override List<SteamBuildConfig> GetAllData()
            {
                GetSteamBuildData();
                return data.Configs;
            }
        }

        internal class SteamBranchPopup : CustomMultiDropdown<SteamBuildConfig, SteamBuildBranch>
        {
            public override List<(SteamBuildConfig, List<SteamBuildBranch>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToBranches();
            }
        }

        private static SteamBuildData data;

        public static SteamBuildData GetSteamBuildData()
        {
            if (data == null)
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    data = JsonUtility.FromJson<SteamBuildData>(json);
                    if (data == null)
                    {
                        Debug.Log("Config is null. Creating new config");
                        data = new SteamBuildData();
                        Save();
                    }

                    // v1.1.3 added IDs and changed branches to a class so we need to migrate them for previous users
                    for (var i = 0; i < data.Configs.Count; i++)
                    {
                        var config = data.Configs[i];
                        if (config.ID == 0){
                            config.ID = i + 1;
                        }

#pragma warning disable CS0618 // Type or member is obsolete
                        if (config.Branches != null && config.Branches.Count > 0)
                        {
                            config.ConfigBranches = new List<SteamBuildBranch>();
                            for (var j = 0; j < config.Branches.Count; j++)
                            {
                                config.ConfigBranches.Add(new SteamBuildBranch(j + 1, config.Branches[j]));
                            }

                            config.Branches = null;
                        }
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
                else
                {
                    Debug.Log("SteamBuildData does not exist. Creating new file");
                    data = new SteamBuildData();
                    Save();
                }
            }

            return data;
        }

        public static void Save()
        {
            if (data != null)
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                if (!File.Exists(FilePath))
                {
                    var stream = File.Create(FilePath);
                    stream.Close();
                }

                File.WriteAllText(FilePath, json);
            }
        }

        public static SteamConfigPopup ConfigPopup => m_configPopup ?? (m_configPopup = new SteamConfigPopup());
        private static SteamConfigPopup m_configPopup;

        public static SteamDepotPopup DepotPopup => m_depotPopup ?? (m_depotPopup = new SteamDepotPopup());
        private static SteamDepotPopup m_depotPopup;

        public static SteamBranchPopup BranchPopup => m_branchPopup ?? (m_branchPopup = new SteamBranchPopup());
        private static SteamBranchPopup m_branchPopup;
    }
}