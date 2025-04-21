using System.IO;
using UnityEditor;

namespace Wireframe
{
    /// <summary>
    /// User is able to select a file to upload
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    public class FileSource : ABrowsePathSource
    {
        public override string DisplayName => "File";
        protected override string ButtonText => "Choose file Upload...";

        public FileSource(string path) : base(null, path)
        {
        }
        
        internal FileSource(BuildUploaderWindow window) : base(window)
        {
        }

        protected override string SelectFile()
        {
            return EditorUtility.OpenFilePanel("Select file to Upload", "", "");
        }
    }
}