using System;
using UnityEditor;

namespace Wireframe
{
    public partial class AppleConfig
    {
        /// <summary>
        /// An App Store Connect API Key.
        ///
        /// IssuerID and KeyID are identifying (non-secret) and are stored in the JSON config.
        /// The .p8 private key file path is per-machine. The file itself is not copied.
        /// We read it on demand at upload/JWT-signing time so users can rotate keys without touching this asset.
        /// </summary>
        [Serializable]
        public class AppleApiKey : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            /// <summary>
            /// Local path to the AuthKey_{KeyID}.p8 file. Stored per-machine in EditorPrefs
            /// because the file lives outside the project on the developer's machine.
            /// </summary>
            public string PrivateKeyPath
            {
                get => EditorPrefs.GetString("AppleApiKeyP8Path_" + Name, "");
                set => EditorPrefs.SetString("AppleApiKeyP8Path_" + Name, value);
            }

            public string Name;
            public string IssuerID;
            public string KeyID;

            private int m_id;

            public AppleApiKey()
            {
                m_id = 0;
                Name = "Template";
                IssuerID = "";
                KeyID = "";
            }

            public AppleApiKey(int id, string displayName)
            {
                m_id = id;
                Name = displayName;
                IssuerID = "";
                KeyID = "";
            }
        }
    }
}
