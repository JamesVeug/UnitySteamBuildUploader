using System.IO;

namespace Wireframe
{
    [Wiki("Exclude Folders", "modifiers", "Exclude/delete folders from the build by specifying a regex pattern.")]
    [BuildModifier("Exclude Folders")]
    public class ExcludeFoldersByRegex_BuildModifier : AExcludePathsByRegex_BuildModifier
    {
        protected override string ListHeader => "Exclude Folders";

        public ExcludeFoldersByRegex_BuildModifier() : base()
        {
            
        }

        public ExcludeFoldersByRegex_BuildModifier(params Selection[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersByRegex_BuildModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersByRegex_BuildModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false) : base(fileRegex, recursive, searchAllDirectories)
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