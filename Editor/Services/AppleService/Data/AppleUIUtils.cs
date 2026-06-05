using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class AppleUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/AppleConfig.json";

        private static AppleConfig data;

        public static AppleConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("AppleConfig does not exist. Creating new file");
                    data = new AppleConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JSON.DeserializeObject<AppleConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new AppleConfig();
                data.Initialize();
                Save();
            }
            else
            {
                for (var i = 0; i < data.apiKeys.Count; i++)
                {
                    data.apiKeys[i].Id = i + 1;
                }

                for (var i = 0; i < data.apps.Count; i++)
                {
                    data.apps[i].Id = i + 1;
                    for (var j = 0; j < data.apps[i].betaGroups.Count; j++)
                    {
                        data.apps[i].betaGroups[j].Id = j + 1;
                    }
                }
            }
        }

        public static void Save()
        {
            if (data != null)
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JSON.SerializeObject(data);
                if (!File.Exists(FilePath))
                {
                    var stream = File.Create(FilePath);
                    stream.Close();
                }

                File.WriteAllText(FilePath, json);
            }
        }

        public class AppleApiKeyPopup : CustomDropdown<AppleConfig.AppleApiKey>
        {
            public override string FirstEntryText => "Choose API Key";

            protected override List<AppleConfig.AppleApiKey> FetchAllData()
            {
                GetConfig();
                return data.apiKeys;
            }
        }

        public class AppleAppPopup : CustomDropdown<AppleConfig.AppleApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<AppleConfig.AppleApp> FetchAllData()
            {
                GetConfig();
                return data.apps;
            }
        }

        public class AppleBetaGroupPopup : CustomMultiDropdown<AppleConfig.AppleApp, AppleConfig.AppleBetaGroup>
        {
            public override string FirstEntryText => "Choose Beta Group";

            public override List<(AppleConfig.AppleApp, List<AppleConfig.AppleBetaGroup>)> GetAllData()
            {
                GetConfig();

                List<(AppleConfig.AppleApp, List<AppleConfig.AppleBetaGroup>)> dataList =
                    new List<(AppleConfig.AppleApp, List<AppleConfig.AppleBetaGroup>)>();
                foreach (AppleConfig.AppleApp app in data.apps)
                {
                    dataList.Add((app, app.betaGroups));
                }
                return dataList;
            }
        }

        public static AppleApiKeyPopup ApiKeyPopup => m_apiKeyPopup ?? (m_apiKeyPopup = new AppleApiKeyPopup());
        private static AppleApiKeyPopup m_apiKeyPopup;

        public static AppleAppPopup AppPopup => m_appPopup ?? (m_appPopup = new AppleAppPopup());
        private static AppleAppPopup m_appPopup;

        public static AppleBetaGroupPopup BetaGroupPopup => m_betaGroupPopup ?? (m_betaGroupPopup = new AppleBetaGroupPopup());
        private static AppleBetaGroupPopup m_betaGroupPopup;
    }
}
