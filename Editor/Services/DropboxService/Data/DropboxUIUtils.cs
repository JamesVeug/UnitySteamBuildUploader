using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class DropboxUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/DropboxConfig.json";

        private static DropboxConfig data;

        public static DropboxConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("DropboxConfig does not exist. Creating new file");
                    data = new DropboxConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<DropboxConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new DropboxConfig();
                data.Initialize();
                Save();
            }
            else
            {
                if (data.apps == null) data.apps = new List<DropboxConfig.DropboxApp>(2);
                if (data.folders == null) data.folders = new List<DropboxConfig.DropboxFolder>(2);

                for (var i = 0; i < data.apps.Count; i++)
                {
                    data.apps[i].Id = i + 1;
                }

                for (var i = 0; i < data.folders.Count; i++)
                {
                    data.folders[i].Id = i + 1;
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

        public class DropboxAppPopup : CustomDropdown<DropboxConfig.DropboxApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<DropboxConfig.DropboxApp> FetchAllData()
            {
                GetConfig();
                return data.apps;
            }
        }

        public class DropboxFolderPopup : CustomDropdown<DropboxConfig.DropboxFolder>
        {
            public override string FirstEntryText => "Choose Folder";

            protected override List<DropboxConfig.DropboxFolder> FetchAllData()
            {
                GetConfig();
                return data.folders;
            }
        }

        public static DropboxAppPopup AppPopup => m_appPopup ?? (m_appPopup = new DropboxAppPopup());
        private static DropboxAppPopup m_appPopup;

        public static DropboxFolderPopup FolderPopup => m_folderPopup ?? (m_folderPopup = new DropboxFolderPopup());
        private static DropboxFolderPopup m_folderPopup;
    }
}
