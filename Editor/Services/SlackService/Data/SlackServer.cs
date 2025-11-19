using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class SlackConfig
    {
        [Serializable]
        public class SlackServer : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public int ServerID;
            public List<SlackChannel> channels;
            
            private int m_id;
            
            public SlackServer()
            {
                m_id = 0;
                Name = "Template";
                
                ServerID = 0;
                channels = new List<SlackChannel>(2);
            }
            
            public SlackServer(int id, string displayName, int serverId)
            {
                m_id = id;
                Name = displayName;
                
                ServerID = serverId;
                channels = new List<SlackChannel>(2);
            }
        }
        
        [Serializable]
        public class SlackChannel : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public string ChannelID;
            
            private int m_id;
            
            public SlackChannel()
            {
                m_id = 0;
                Name = "Template";
                ChannelID = "";
            }
            
            public SlackChannel(int id, string displayName, string channelID)
            {
                m_id = id;
                Name = displayName;
                ChannelID = channelID;
            }
        }
    }
}