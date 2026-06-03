using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class GoogleUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/GoogleConfig.json";

        private static GoogleConfig data;

        public static GoogleConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("GoogleConfig does not exist. Creating new file");
                    data = new GoogleConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<GoogleConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new GoogleConfig();
                data.Initialize();
                Save();
            }
            else
            {
                if (data.apps == null) data.apps = new List<GoogleConfig.GoogleApp>(2);
                if (data.driveFolders == null) data.driveFolders = new List<GoogleConfig.GoogleDriveFolder>(2);
                if (data.chatSpaces == null) data.chatSpaces = new List<GoogleConfig.GoogleChatSpace>(2);
                if (data.playApps == null) data.playApps = new List<GoogleConfig.GooglePlayApp>(2);

                for (var i = 0; i < data.apps.Count; i++)
                {
                    data.apps[i].Id = i + 1;
                }

                for (var i = 0; i < data.driveFolders.Count; i++)
                {
                    data.driveFolders[i].Id = i + 1;
                }

                for (var i = 0; i < data.chatSpaces.Count; i++)
                {
                    data.chatSpaces[i].Id = i + 1;
                }

                for (var i = 0; i < data.playApps.Count; i++)
                {
                    data.playApps[i].Id = i + 1;
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

        public class GoogleAppPopup : CustomDropdown<GoogleConfig.GoogleApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<GoogleConfig.GoogleApp> FetchAllData()
            {
                GetConfig();
                return data.apps;
            }
        }

        public class GoogleDriveFolderPopup : CustomDropdown<GoogleConfig.GoogleDriveFolder>
        {
            public override string FirstEntryText => "Choose Folder";

            protected override List<GoogleConfig.GoogleDriveFolder> FetchAllData()
            {
                GetConfig();
                return data.driveFolders;
            }
        }

        public class GoogleChatSpacePopup : CustomDropdown<GoogleConfig.GoogleChatSpace>
        {
            public override string FirstEntryText => "Choose Space";

            protected override List<GoogleConfig.GoogleChatSpace> FetchAllData()
            {
                GetConfig();
                return data.chatSpaces;
            }
        }

        public class GooglePlayAppPopup : CustomDropdown<GoogleConfig.GooglePlayApp>
        {
            public override string FirstEntryText => "Choose Play App";

            protected override List<GoogleConfig.GooglePlayApp> FetchAllData()
            {
                GetConfig();
                return data.playApps;
            }
        }

        public static GoogleAppPopup AppPopup => m_appPopup ?? (m_appPopup = new GoogleAppPopup());
        private static GoogleAppPopup m_appPopup;

        public static GoogleDriveFolderPopup DriveFolderPopup => m_driveFolderPopup ?? (m_driveFolderPopup = new GoogleDriveFolderPopup());
        private static GoogleDriveFolderPopup m_driveFolderPopup;

        public static GoogleChatSpacePopup ChatSpacePopup => m_chatSpacePopup ?? (m_chatSpacePopup = new GoogleChatSpacePopup());
        private static GoogleChatSpacePopup m_chatSpacePopup;

        public static GooglePlayAppPopup PlayAppPopup => m_playAppPopup ?? (m_playAppPopup = new GooglePlayAppPopup());
        private static GooglePlayAppPopup m_playAppPopup;
    }
}
