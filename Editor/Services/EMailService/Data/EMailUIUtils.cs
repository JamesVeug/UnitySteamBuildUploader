using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class EMailUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/EMailConfig.json";

        private static EMailConfig data;

        public static EMailConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("EMailConfig does not exist. Creating new file");
                    data = new EMailConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<EMailConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new EMailConfig();
                data.Initialize();
                Save();
            }
            else
            {
                if (data.accounts == null)
                {
                    data.accounts = new List<EMailConfig.EMailAccount>(2);
                }

                for (var i = 0; i < data.accounts.Count; i++)
                {
                    data.accounts[i].Id = i + 1;
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

        public class EMailAccountPopup : CustomDropdown<EMailConfig.EMailAccount>
        {
            public override string FirstEntryText => "Choose Account";

            protected override List<EMailConfig.EMailAccount> FetchAllData()
            {
                GetConfig();
                return data.accounts;
            }
        }

        public static EMailAccountPopup AccountPopup => m_accountPopup ?? (m_accountPopup = new EMailAccountPopup());
        private static EMailAccountPopup m_accountPopup;
    }
}
