using System.IO;

namespace Wireframe
{
    public class ExcludeFilesByRegex_BuildModifier : AExcludePathsByRegex_BuildModifier
    {
        public ExcludeFilesByRegex_BuildModifier() : base()
        {
            
        }
        
        public ExcludeFilesByRegex_BuildModifier(params Selection[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesByRegex_BuildModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesByRegex_BuildModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false) : base(fileRegex, recursive, searchAllDirectories)
        {
            
        }

        protected override string[] GetFiles(string cachedDirectory, Selection regex)
        {
            SearchOption searchOption = regex.SearchAllDirectories ? 
                SearchOption.AllDirectories : 
                SearchOption.TopDirectoryOnly;
            
            string[] files = Directory.GetFiles(cachedDirectory, regex.Regex, searchOption);
            return files;
        }
    }
}