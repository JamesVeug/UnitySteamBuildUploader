using System;
using UnityEditor;

namespace Wireframe
{
    public partial class GoogleConfig
    {
        /// <summary>
        /// A named Google Chat space
        /// </summary>
        [Serializable]
        public class GoogleChatSpace : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string WebhookURL
            {
                get => EditorPrefs.GetString("GoogleChatSpaceWebhook_" + Name, "");
                set => EditorPrefs.SetString("GoogleChatSpaceWebhook_" + Name, value);
            }

            public string Name;

            private int m_id;

            public GoogleChatSpace()
            {
                m_id = 0;
                Name = "Template";
            }

            public GoogleChatSpace(int id, string displayName)
            {
                m_id = id;
                Name = displayName;
            }
        }
    }
}
