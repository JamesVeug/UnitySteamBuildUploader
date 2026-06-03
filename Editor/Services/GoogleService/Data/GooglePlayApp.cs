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

            /// <summary>Application package name (e.g. com.example.MyGame). Required for every Play API call.</summary>
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

    /// <summary>
    /// Tracks defined by the Google Play Developer API for releases.
    /// https://developers.google.com/android-publisher/tracks
    /// </summary>
    public enum GooglePlayTrack
    {
        Internal = 0,
        Alpha = 1,
        Beta = 2,
        Production = 3,
    }
}
