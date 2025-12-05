using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    internal static partial class SlackUIUtils
    {
        private static readonly string FilePath = Application.dataPath + "/../BuildUploader/SlackConfig.json";

        private static SlackConfig data;

        public static SlackConfig GetConfig()
        {
            if (data == null)
            {
                if (File.Exists(FilePath))
                {
                    LoadFile(FilePath);
                }
                else
                {
                    Debug.Log("SlackConfig does not exist. Creating new file");
                    data = new SlackConfig();
                    data.Initialize();
                    Save();
                }
            }

            return data;
        }

        private static void LoadFile(string path)
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SlackConfig>(json);
            if (data == null)
            {
                Debug.Log("Config has bad json so creating new config");
                data = new SlackConfig();
                data.Initialize();
                Save();
            }
            else
            {
                for (var i = 0; i < data.apps.Count; i++)
                {
                    SlackConfig.SlackApp app = data.apps[i];
                    app.Id = i + 1;
                }

                for (var i = 0; i < data.servers.Count; i++)
                {
                    data.servers[i].Id = i + 1;
                    for (var j = 0; j < data.servers[i].channels.Count; j++)
                    {
                        data.servers[i].channels[j].Id = j + 1;
                    }
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
        
        public class SlackAppPopup : CustomDropdown<SlackConfig.SlackApp>
        {
            public override string FirstEntryText => "Choose App";

            protected override List<SlackConfig.SlackApp> FetchAllData()
            {
                GetConfig();
                return data.apps;
            }
        }
        
        public class SlackServerPopup : CustomDropdown<SlackConfig.SlackServer>
        {
            public override string FirstEntryText => "Choose Server";

            protected override List<SlackConfig.SlackServer> FetchAllData()
            {
                GetConfig();
                return data.servers;
            }
        }
        
        
        
        public class SlackChannelPopup : CustomMultiDropdown<SlackConfig.SlackServer, SlackConfig.SlackChannel>
        {
            public override string FirstEntryText => "Choose Channel";
            
            public override List<(SlackConfig.SlackServer, List<SlackConfig.SlackChannel>)> GetAllData()
            {
                GetConfig();
                
                List<(SlackConfig.SlackServer, List<SlackConfig.SlackChannel>)> dataList = new List<(SlackConfig.SlackServer, List<SlackConfig.SlackChannel>)>();
                foreach (SlackConfig.SlackServer server in data.servers)
                {
                    dataList.Add((server, server.channels));
                }
                return dataList;
            }
        }
        
        public static SlackAppPopup AppPopup => m_appPopup ?? (m_appPopup = new SlackAppPopup());
        private static SlackAppPopup m_appPopup;
        
        public static SlackServerPopup ServerPopup => m_serverPopup ?? (m_serverPopup = new SlackServerPopup());
        private static SlackServerPopup m_serverPopup;
        
        public static SlackChannelPopup ChannelPopup => m_channelPopup ?? (m_channelPopup = new SlackChannelPopup());
        private static SlackChannelPopup m_channelPopup;
    }
}