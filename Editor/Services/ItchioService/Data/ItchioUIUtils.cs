using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class ItchioUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/ItchioConfig.json";

        private static ItchioAppData data;

        public static ItchioAppData GetItchioBuildData(bool createIfMissing = true)
        {
            if (data == null && createIfMissing)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("ItchioConfig does not exist. Creating new file");
                    data = new ItchioAppData();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<ItchioAppData>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new ItchioAppData();
                data.Initialize();
                Save();
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

        public static ItchioUserPopup UserPopup => m_userPopup ?? (m_userPopup = new ItchioUserPopup());
        private static ItchioUserPopup m_userPopup;
        
        
        public static ItchioGamePopup GamePopup => m_gamePopup ?? (m_gamePopup = new ItchioGamePopup());
        private static ItchioGamePopup m_gamePopup;
    }
    
    [Serializable]
    public class ItchioChannel : DropdownElement
    {
        public static readonly string[] DefaultChannels = new[]
        {
            "windows",
            "mac",
            "linux",
            "webgl",
            "android"
        };
        
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string Name;
        
        public ItchioChannel()
        {
            ID = 0;
            Name = "Template";
        }
        
        public ItchioChannel(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }

    internal class ItchioAppData
    {
        public List<ItchioUser> Users = new List<ItchioUser>();
        
        public List<ItchioChannel> Channels = new List<ItchioChannel>();

        public void Initialize()
        {
            Users = new List<ItchioUser>(2);
            Channels = new List<ItchioChannel>(ItchioChannel.DefaultChannels.Length);
            for (var i = 0; i < ItchioChannel.DefaultChannels.Length; i++)
            {
                var channel = ItchioChannel.DefaultChannels[i];
                Channels.Add(new ItchioChannel(i+1, channel));
            }
        }

        public List<(ItchioUser, List<ItchioGameData>)> UserToGames()
        {
            var userChannels = new List<(ItchioUser, List<ItchioGameData>)>();
            foreach (ItchioUser user in Users)
            {
                userChannels.Add((user, user.GameIds));
            }
            return userChannels;
        }
    }

    [Serializable]
    internal class ItchioUser : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string Name;
        public List<ItchioGameData> GameIds;
        
        public ItchioUser()
        {
            ID = 0;
            Name = "Template";
            GameIds = new List<ItchioGameData>();
        }
        
        public ItchioUser(int id, string name)
        {
            ID = id;
            Name = name;
            GameIds = new List<ItchioGameData>();
        }
    }

    [Serializable]
    public class ItchioGameData : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string Name;
        
        public ItchioGameData()
        {
            ID = 0;
            Name = "Template";
        }
        
        public ItchioGameData(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}