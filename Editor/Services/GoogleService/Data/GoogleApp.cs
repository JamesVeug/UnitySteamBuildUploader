using System;
using UnityEditor;

namespace Wireframe
{
    public partial class GoogleConfig
    {
        [Serializable]
        public class GoogleApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Token
            {
                get => EditorPrefs.GetString("GoogleAppToken_" + Name, "");
                set => EditorPrefs.SetString("GoogleAppToken_" + Name, value);
            }

            public string Name;

            private int m_id;

            public GoogleApp()
            {
                m_id = 0;
                Name = "Template";
            }

            public GoogleApp(int id, string displayName)
            {
                m_id = id;
                Name = displayName;
            }
        }
    }
}
