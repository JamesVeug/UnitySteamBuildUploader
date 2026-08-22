using System;

namespace Wireframe
{
    public partial class SlackConfig
    {
        [Serializable]
        public class SlackApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Token
            {
                get => EncodedEditorPrefs.GetString($"SlackAppToken_{Name}", "");
                set => EncodedEditorPrefs.SetString($"SlackAppToken_{Name}", value);
            }

            public string Name;
            
            private int m_id;
            
            public SlackApp()
            {
                m_id = 0;
                Name = "Template";
            }
            
            public SlackApp(int id, string displayName, bool isBot = true)
            {
                m_id = id;
                Name = displayName;
            }
        }
    }
}