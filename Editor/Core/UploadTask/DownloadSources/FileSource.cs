using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a file to upload
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(FileSource), "sources", "Choose a File to upload")]
    [UploadSource("File", "Choose file Upload...", false)]
    public class FileSource : ABrowsePathSource
    {
        public FileSource() : base()
        {
            // Required for reflection
        }
        
        public FileSource(string path) : base(path)
        {
        }

        protected internal override string SelectFile()
        {
            return EditorUtility.OpenFilePanel("Select file to Upload", "", "");
        }
    }
}