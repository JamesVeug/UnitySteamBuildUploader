using System;
using UnityEditor;

namespace Wireframe
{
    public partial class GoogleConfig
    {
        /// <summary>
        /// A labelled OAuth2 access token used to authenticate against Google APIs
        /// (currently used by the Google Drive destination).
        ///
        /// The token itself is stored in EditorPrefs - it must never be written to
        /// the serialized JSON config so it does not leak through source control.
        /// </summary>
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
