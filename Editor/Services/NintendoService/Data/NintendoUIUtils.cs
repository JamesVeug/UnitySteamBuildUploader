using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static class NintendoUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/NintendoConfig.json";

        public class NintendoConfigPopup : CustomDropdown<NintendoApp>
        {
            public override string FirstEntryText => "Choose Title";

            protected override List<NintendoApp> FetchAllData()
            {
                GetNintendoBuildData();
                return data.Configs;
            }
        }

        public class NintendoBranchPopup : CustomMultiDropdown<NintendoApp, NintendoBranch>
        {
            public override string FirstEntryText => "Choose Branch";

            public override List<(NintendoApp, List<NintendoBranch>)> GetAllData()
            {
                GetNintendoBuildData();
                return data.ConfigToBranches();
            }
        }

        private static NintendoAppData data;

        public static NintendoAppData GetNintendoBuildData(bool createIfNotExists = true)
        {
            if (data == null && createIfNotExists)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("NintendoBuildData does not exist. Creating new file");
                    data = new NintendoAppData();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<NintendoAppData>(json);
            if (data == null)
            {
                Debug.Log("Config is null. Creating new config");
                data = new NintendoAppData();
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

        public static NintendoConfigPopup ConfigPopup => m_configPopup ?? (m_configPopup = new NintendoConfigPopup());
        private static NintendoConfigPopup m_configPopup;

        public static NintendoBranchPopup BranchPopup => m_branchPopup ?? (m_branchPopup = new NintendoBranchPopup());
        private static NintendoBranchPopup m_branchPopup;
    }
}
