using System;

namespace Wireframe
{
    public partial class DropboxConfig
    {
        /// <summary>
        /// A named Dropbox destination folder. Path is the folder path inside the Dropbox
        /// app/account (e.g. /Builds). An empty Path uploads to the root of the app folder.
        /// </summary>
        [Serializable]
        public class DropboxFolder : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public string Path;

            private int m_id;

            public DropboxFolder()
            {
                m_id = 0;
                Name = "Template";
                Path = "";
            }

            public DropboxFolder(int id, string displayName, string path)
            {
                m_id = id;
                Name = displayName;
                Path = path;
            }
        }
    }
}
