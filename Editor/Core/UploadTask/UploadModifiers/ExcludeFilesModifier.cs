using System.IO;

namespace Wireframe
{
    [Wiki("ExcludeFilesModifier", "modifiers", "Exclude/delete files from the build by specifying a regex pattern.")]
    [UploadModifier("Exclude Files")]
    public class ExcludeFilesModifier : AExcludePathsByRegex_UploadModifier
    {
        protected override string ListHeader => "Exclude Files";
        
        public ExcludeFilesModifier() : base()
        {
            
        }
        
        public ExcludeFilesModifier(params Selection[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false) : base(fileRegex, recursive, searchAllDirectories)
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

        public override string Summary()
        {
            return "Removing files"; // NOTE: Will only show when ModifyBuildAtPath is executing
        }
    }
}