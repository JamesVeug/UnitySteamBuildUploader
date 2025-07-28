using System.IO;

namespace Wireframe
{
    [Wiki("ExcludeFoldersModifier", "modifiers", "Exclude/delete folders from the build by specifying a regex pattern.")]
    [UploadModifier("Exclude Folders")]
    public class ExcludeFoldersModifier : AExcludePathsByRegex_UploadModifier
    {
        protected override string ListHeader => "Exclude Folders";

        public ExcludeFoldersModifier() : base()
        {
            
        }

        public ExcludeFoldersModifier(params Selection[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false) : base(fileRegex, recursive, searchAllDirectories)
        {
            
        }

        protected override string[] GetFiles(string cachedDirectory, Selection regex)
        {
            SearchOption searchOption = regex.SearchAllDirectories ? 
                SearchOption.AllDirectories : 
                SearchOption.TopDirectoryOnly;
            
            string[] files = Directory.GetDirectories(cachedDirectory, regex.Regex, searchOption);
            return files;
        }
    }
}