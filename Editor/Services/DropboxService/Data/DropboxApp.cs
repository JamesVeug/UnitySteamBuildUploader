using System;
using UnityEditor;

namespace Wireframe
{
    public partial class DropboxConfig
    {
        /// <summary>
        /// A labelled Dropbox access token used to authenticate against the Dropbox API
        /// (used by the Dropbox upload destination).
        ///
        /// The token itself is stored in EditorPrefs - it must never be written to the
        /// serialized JSON config so it does not leak through source control.
        /// </summary>
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
