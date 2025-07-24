using System;
using System.Collections.Generic;
using UnityEditor;

namespace Wireframe
{
    public partial class DiscordConfig
    {
        [Serializable]
        public class DiscordServer : DropdownElement
        {
            public int Id => ID;
            public string DisplayName => Name;

            public int ID;
            public string Name;
            public int ServerID;
            public List<DiscordChannel> channels;
            
            public DiscordServer()
            {
                ID = 0;
                Name = "Template";
                
                ServerID = 0;
                channels = new List<DiscordChannel>(2);
            }
            
            public DiscordServer(int id, string displayName, int serverId)
            {
                ID = id;
                Name = displayName;
                
                ServerID = serverId;
                channels = new List<DiscordChannel>(2);
            }
        }
        
        [Serializable]
        public class DiscordChannel : DropdownElement
        {
            public int Id => ID;
            public string DisplayName => Name;

            private int ID;
            public string Name;
            public long ChannelID;
            
            public DiscordChannel()
            {
                ID = 0;
                Name = "Template";
                ChannelID = 0;
            }
            
            public DiscordChannel(int id, string displayName, long channelID)
            {
                ID = id;
                Name = displayName;
                ChannelID = channelID;
            }
        }
    }
}