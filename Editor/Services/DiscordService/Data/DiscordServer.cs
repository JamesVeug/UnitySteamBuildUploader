using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class DiscordConfig
    {
        [Serializable]
        public class DiscordServer : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public long ServerID;
            public List<DiscordChannel> channels;
            
            private int m_id;
            
            public DiscordServer()
            {
                m_id = 0;
                Name = "Template";
                
                ServerID = 0;
                channels = new List<DiscordChannel>(2);
            }
            
            public DiscordServer(int id, string displayName, long serverId)
            {
                m_id = id;
                Name = displayName;
                
                ServerID = serverId;
                channels = new List<DiscordChannel>(2);
            }
        }
        
        [Serializable]
        public class DiscordChannel : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public long ChannelID;
            
            private int m_id;
            
            public DiscordChannel()
            {
                m_id = 0;
                Name = "Template";
                ChannelID = 0;
            }
            
            public DiscordChannel(int id, string displayName, long channelID)
            {
                m_id = id;
                Name = displayName;
                ChannelID = channelID;
            }
        }
    }
}