using System;
using UnityEditor;

namespace Wireframe
{
    public partial class DropboxConfig
    {
        [Serializable]
        public class DropboxApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Token
            {
                get => EditorPrefs.GetString("DropboxAppToken_" + Name, "");
                set => EditorPrefs.SetString("DropboxAppToken_" + Name, value);
            }

            public string Name;

            private int m_id;

            public DropboxApp()
            {
                m_id = 0;
                Name = "Template";
            }

            public DropboxApp(int id, string displayName)
            {
                m_id = id;
                Name = displayName;
            }
        }
    }
}
