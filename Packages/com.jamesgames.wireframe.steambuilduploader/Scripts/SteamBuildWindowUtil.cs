using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    public static class SteamBuildWindowUtil
    {
        private static readonly string FilePath = Application.dataPath + "/../SteamBuilder/SteamWorksConfig.json";

        public class SteamDepotPopup : CustomMultiDropdown<SteamBuildConfig, SteamBuildDepot>
        {
            public override List<(SteamBuildConfig, List<SteamBuildDepot>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToDepots();
            }

            public override string ItemDisplayName(SteamBuildDepot y)
            {
                return y.Name;
            }
        }

        public class SteamConfigPopup : CustomDropdown<SteamBuildConfig>
        {
            public override List<SteamBuildConfig> GetAllData()
            {
                GetSteamBuildData();
                return data.Configs;
            }

            public override string ItemDisplayName(SteamBuildConfig y)
            {
                return y.Name;
            }
        }

        public class SteamBranchPopup : CustomMultiDropdown<SteamBuildConfig, string>
        {
            public override List<(SteamBuildConfig, List<string>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToBranches();
            }

            public override string ItemDisplayName(string y)
            {
                return y;
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