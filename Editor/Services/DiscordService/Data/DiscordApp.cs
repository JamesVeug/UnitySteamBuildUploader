using System;
using UnityEditor;

namespace Wireframe
{
    public partial class DiscordConfig
    {
        [Serializable]
        public class DiscordApp : DropdownElement
        {
            public int Id => ID;
            public string DisplayName => Name;

            public string Token
            {
                get => EditorPrefs.GetString("DiscordAppToken_" + Name, "");
                set => EditorPrefs.SetString("DiscordAppToken_" + Name, value);
            }

            private int ID;
            public string Name;
            public bool IsBot;
            
            public DiscordApp()
            {
                ID = 0;
                Name = "Template";
                IsBot = true;
            }
            
            public DiscordApp(int id, string displayName, bool isBot = true)
            {
                ID = id;
                Name = displayName;
                IsBot = isBot;
            }
        }
    }
}