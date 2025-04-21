using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static class SteamUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/SteamWorksConfig.json";

        public class SteamDepotPopup : CustomMultiDropdown<SteamApp, SteamDepot>
        {
            public override string FirstEntryText => "Choose Depot";
            
            public override List<(SteamApp, List<SteamDepot>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToDepots();
            }
        }

        public class SteamConfigPopup : CustomDropdown<SteamApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<SteamApp> FetchAllData()
            {
                GetSteamBuildData();
                return data.Configs;
            }
        }

        public class SteamBranchPopup : CustomMultiDropdown<SteamApp, SteamBranch>
        {
            public override string FirstEntryText => "Choose Branch";
            
            public override List<(SteamApp, List<SteamBranch>)> GetAllData()
            {
                GetSteamBuildData();
                return data.ConfigToBranches();
            }
        }

        private static SteamAppData data;

        public static SteamAppData GetSteamBuildData()
        {
            if (data == null)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else if (File.Exists(Application.dataPath + "/../SteamBuilder/SteamWorksConfig.json"))
                {
                    Debug.Log("Found SteamWorksConfig.json from a previous version. Migrating to new version");
                    LoadFile(Application.dataPath + "/../SteamBuilder/SteamWorksConfig.json");
                    Save();
                }
                else
                {
                    Debug.Log("SteamBuildData does not exist. Creating new file");
                    data = new SteamAppData();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SteamAppData>(json);
            if (data == null)
            {
                Debug.Log("Config is null. Creating new config");
                data = new SteamAppData();
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
                    config.ConfigBranches = new List<SteamBranch>();
                    for (var j = 0; j < config.Branches.Count; j++)
                    {
                        config.ConfigBranches.Add(new SteamBranch(j + 1, config.Branches[j]));
                    }

                    config.Branches = null;
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
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

        public static SteamDepotPopup DepotPopup => _mDepotPopup ?? (_mDepotPopup = new SteamDepotPopup());
        private static SteamDepotPopup _mDepotPopup;

        public static SteamBranchPopup BranchPopup => m_branchPopup ?? (m_branchPopup = new SteamBranchPopup());
        private static SteamBranchPopup m_branchPopup;
    }
}