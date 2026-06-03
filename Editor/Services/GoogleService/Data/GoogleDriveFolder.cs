using System;

namespace Wireframe
{
    public partial class GoogleConfig
    {
        /// <summary>
        /// A named Google Drive destination folder. FolderId is the value found in the
        /// Drive folder URL (https://drive.google.com/drive/folders/&lt;FolderId&gt;).
        /// An empty FolderId uploads to the root of "My Drive".
        /// </summary>
        [Serializable]
        public class GoogleDriveFolder : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;
            public string FolderId;

            private int m_id;

            public GoogleDriveFolder()
            {
                m_id = 0;
                Name = "Template";
                FolderId = "";
            }

            public GoogleDriveFolder(int id, string displayName, string folderId)
            {
                m_id = id;
                Name = displayName;
                FolderId = folderId;
            }
        }
    }
}
