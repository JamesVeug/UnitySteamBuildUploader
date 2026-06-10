using System;
using UnityEditor;

namespace Wireframe
{
    public partial class XboxConfig
    {
        [Serializable]
        public class XboxApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            /// <summary>
            /// Client secret for the Azure AD app registration.
            /// Stored in EditorPrefs — never committed to source control.
            /// </summary>
            public string ClientSecret
            {
                get => EditorPrefs.GetString("XboxClientSecret_" + Name, "");
                set => EditorPrefs.SetString("XboxClientSecret_" + Name, value);
            }

            /// <summary>Display name for this app entry.</summary>
            public string Name;

            /// <summary>
            /// Microsoft Store Application ID (e.g. "1ABCDEFGHI2").
            /// Found in Partner Center → App identity. Safe to commit.
            /// </summary>
            public string ProductId;

            /// <summary>
            /// Azure Active Directory tenant ID (GUID).
            /// Found in Azure Portal → Azure Active Directory → Overview. Safe to commit.
            /// </summary>
            public string TenantId;

            /// <summary>
            /// Azure AD app registration client ID (GUID).
            /// Found in Azure Portal → App registrations. Safe to commit.
            /// </summary>
            public string ClientId;

            private int m_id;

            public XboxApp()
            {
                m_id    = 0;
                Name      = "My Xbox App";
                ProductId = "";
                TenantId  = "";
                ClientId  = "";
            }

            public XboxApp(int id, string displayName)
            {
                m_id      = id;
                Name      = displayName;
                ProductId = "";
                TenantId  = "";
                ClientId  = "";
            }
        }
    }
}
