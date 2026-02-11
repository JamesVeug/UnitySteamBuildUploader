using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Wireframe
{
    [Wiki("ExcludeFilesModifier", "modifiers", "Exclude/delete files from the build by specifying a regex pattern.")]
    [UploadModifier("Exclude Files")]
    public class ExcludeFilesModifier : AExcludePathsByRegex_UploadModifier
    {
        protected override string ListHeader => "Exclude Files";
        
        public ExcludeFilesModifier() 
            : base()
        {
            
        }
        
        public ExcludeFilesModifier(WhenToExclude whenToExclude)
            : base(whenToExclude)
        {
            
        }
        
        public ExcludeFilesModifier(WhenToExclude whenToExclude, params Selection[] fileRegexes)
            : base(whenToExclude, fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(params Selection[] fileRegexes)
            : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(WhenToExclude whenToExclude, params string[] fileRegexes)
            : base(whenToExclude, fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(params string[] fileRegexes) : base(fileRegexes)
        {
            
        }
        
        public ExcludeFilesModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false, WhenToExclude whenToExclude = WhenToExclude.DoNotCopyFromSource)
            : base(fileRegex, recursive, searchAllDirectories, whenToExclude)
        {
            
        }

        protected override string[] GetFiles(string cachedDirectory, Selection regex)
        {
            SearchOption searchOption = regex.SearchAllDirectories ? 
                SearchOption.AllDirectories : 
                SearchOption.TopDirectoryOnly;
            
            string[] paths = Directory.GetFiles(cachedDirectory, "*.*", searchOption);
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
            return "Removing files"; // NOTE: Will only show when ModifyBuildAtPath is executing
        }
    }
}