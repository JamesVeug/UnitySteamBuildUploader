using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Wireframe
{
    [Wiki("ExcludeFoldersModifier", "modifiers", "Exclude/delete folders from the build by specifying a regex pattern.")]
    [UploadModifier("Exclude Folders")]
    public class ExcludeFoldersModifier : AExcludePathsByRegex_UploadModifier
    {
        protected override string ListHeader => "Exclude Folders";
        
        public ExcludeFoldersModifier() 
            : base()
        {
            
        }

        public ExcludeFoldersModifier(WhenToExclude whenToExclude)
            : base(whenToExclude)
        {
            
        }
        
        public ExcludeFoldersModifier(WhenToExclude whenToExclude, params Selection[] fileRegexes)
            : base(whenToExclude, fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(params Selection[] fileRegexes)
            : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(WhenToExclude whenToExclude, params string[] fileRegexes)
            : base(whenToExclude, fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFoldersModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false, WhenToExclude whenToExclude = WhenToExclude.DoNotCopyFromSource)
            : base(fileRegex, recursive, searchAllDirectories, whenToExclude)
        {
            
        }

        protected override string[] GetFiles(string cachedDirectory, Selection regex)
        {
            SearchOption searchOption = regex.SearchAllDirectories ? 
                SearchOption.AllDirectories : 
                SearchOption.TopDirectoryOnly;
            
            string[] paths = Directory.GetDirectories(cachedDirectory, "*.*", searchOption);
            List<string> filteredPaths = new List<string>(paths.Length);
            foreach (string path in paths)
            {
                if (Regex.IsMatch(path, regex.Regex))
                {
                    filteredPaths.Add(path);
                }
            }
            
            string[] files = filteredPaths.ToArray();
            return files;
        }

        public override string Summary()
        {
            return "Removing folders"; // NOTE: Will only show when ModifyBuildAtPath is executing
        }
    }
}