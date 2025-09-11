using System;
using UnityEditor;

namespace Wireframe
{
    public partial class DiscordConfig
    {
        [Serializable]
        public class DiscordApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Token
            {
                get => EditorPrefs.GetString("DiscordAppToken_" + Name, "");
                set => EditorPrefs.SetString("DiscordAppToken_" + Name, value);
            }

            public string Name;
            public bool IsBot;
            
            private int m_id;
            
            public DiscordApp()
            {
                m_id = 0;
                Name = "Template";
                IsBot = true;
            }
            
            public DiscordApp(int id, string displayName, bool isBot = true)
            {
                m_id = id;
                Name = displayName;
                IsBot = isBot;
            }
        }
    }
}