using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a folder to upload
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    public class FolderSource : ABrowsePathSource
    {
        public override string DisplayName => "Folder";
        protected override string ButtonText => "Choose Folder to Upload...";

        public FolderSource(string path) : base(null, path)
        {
        }

        internal FolderSource(BuildUploaderWindow window) : base(window)
        {
        }

        protected override string SelectFile()
        {
            return EditorUtility.OpenFolderPanel("Select Folder to upload", m_enteredFilePath, "");
        }
    }
}