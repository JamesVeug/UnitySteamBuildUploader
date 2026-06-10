using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static class XboxUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/XboxConfig.json";

        private static XboxConfig data;

        public static XboxConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("XboxConfig does not exist. Creating new file.");
                    data = new XboxConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<XboxConfig>(json);
            if (data == null)
            {
                Debug.Log("XboxConfig has bad JSON — creating new config.");
                data = new XboxConfig();
                data.Initialize();
                Save();
                return;
            }

            if (data.apps == null)
                data.apps = new List<XboxConfig.XboxApp>();

            // Re-assign sequential IDs to keep them stable
            for (int i = 0; i < data.apps.Count; i++)
                data.apps[i].Id = i + 1;
        }

        public static void Save()
        {
            if (data == null) return;

            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(data, true);
            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Close();
            }

            File.WriteAllText(FilePath, json);
        }

        public static XboxAppPopup AppPopup => m_appPopup ?? (m_appPopup = new XboxAppPopup());
        private static XboxAppPopup m_appPopup;

        public class XboxAppPopup : CustomDropdown<XboxConfig.XboxApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<XboxConfig.XboxApp> FetchAllData()
            {
                GetConfig();
                return data?.apps ?? new List<XboxConfig.XboxApp>();
            }
        }
    }
}
