using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a folder to upload
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [BuildSource("Folder", "Choose Folder Upload...")]
    public class FolderSource : ABrowsePathSource
    {
        public FolderSource() : base()
        {
            // Required for reflection
        }
        
        public FolderSource(string path) : base(path)
        {
        }

        internal override string SelectFile()
        {
            return EditorUtility.OpenFolderPanel("Select Folder to upload", m_enteredFilePath, "");
        }
    }
}