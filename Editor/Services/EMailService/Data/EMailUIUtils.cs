using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class EmailUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/EmailConfig.json";

        private static EmailConfig data;

        public static EmailConfig GetConfig(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("EmailConfig does not exist. Creating new file");
                    data = new EmailConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<EmailConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new EmailConfig();
                data.Initialize();
                Save();
            }
            else
            {
                if (data.accounts == null)
                {
                    data.accounts = new List<EmailConfig.EmailAccount>(2);
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

        public class EmailAccountPopup : CustomDropdown<EmailConfig.EmailAccount>
        {
            public override string FirstEntryText => "Choose Account";

            protected override List<EmailConfig.EmailAccount> FetchAllData()
            {
                GetConfig();
                return data.accounts;
            }
        }

        public static EmailAccountPopup AccountPopup => m_accountPopup ?? (m_accountPopup = new EmailAccountPopup());
        private static EmailAccountPopup m_accountPopup;
    }
}
