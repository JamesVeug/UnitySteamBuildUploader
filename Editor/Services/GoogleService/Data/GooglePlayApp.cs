using System;

namespace Wireframe
{
    public partial class GoogleConfig
    {
        /// <summary>
        /// A Google Play Console application targeted by the Play Developer API.
        /// PackageName is the unique application id (e.g. com.example.MyGame).
        /// </summary>
        [Serializable]
        public class GooglePlayApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public string PackageName;

            private int m_id;

            public GooglePlayApp()
            {
                m_id = 0;
                Name = "Template";
                PackageName = "";
            }

            public GooglePlayApp(int id, string displayName, string packageName)
            {
                m_id = id;
                Name = displayName;
                PackageName = packageName;
            }
        }
    }
}
