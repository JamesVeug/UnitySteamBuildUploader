using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static class PlayStationUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/PlayStationConfig.json";

        public class PlayStationConfigPopup : CustomDropdown<PlayStationApp>
        {
            public override string FirstEntryText => "Choose Title";

            protected override List<PlayStationApp> FetchAllData()
            {
                GetPlayStationBuildData();
                return data.Configs;
            }
        }

        public class PlayStationBranchPopup : CustomMultiDropdown<PlayStationApp, PlayStationBranch>
        {
            public override string FirstEntryText => "Choose Branch";

            public override List<(PlayStationApp, List<PlayStationBranch>)> GetAllData()
            {
                GetPlayStationBuildData();
                return data.ConfigToBranches();
            }
        }

        private static PlayStationAppData data;

        public static PlayStationAppData GetPlayStationBuildData(bool createIfNotExists = true)
        {
            if (data == null && createIfNotExists)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("PlayStationBuildData does not exist. Creating new file");
                    data = new PlayStationAppData();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<PlayStationAppData>(json);
            if (data == null)
            {
                Debug.Log("Config is null. Creating new config");
                data = new PlayStationAppData();
                Save();
            }

            for (var i = 0; i < data.Configs.Count; i++)
            {
                var config = data.Configs[i];
                if (config.ID == 0)
                {
                    config.ID = i + 1;
                }
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

        public static PlayStationConfigPopup ConfigPopup => m_configPopup ?? (m_configPopup = new PlayStationConfigPopup());
        private static PlayStationConfigPopup m_configPopup;

        public static PlayStationBranchPopup BranchPopup => m_branchPopup ?? (m_branchPopup = new PlayStationBranchPopup());
        private static PlayStationBranchPopup m_branchPopup;
    }
}
