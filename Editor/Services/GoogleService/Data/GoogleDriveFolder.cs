using System;

namespace Wireframe
{
    public partial class GoogleConfig
    {
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
