using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_UploadModifier : AUploadModifer
    {
        [Wiki("Regex", "A regex selection to exclude files/folders from the build.")]
        [Serializable]
        public class Selection
        {
            [Wiki("Enabled", "If true, regex will be used to select files/folders for modification.")]
            public bool Enabled = true;
            
            [Wiki("Regex", "Pattern to select files. *.txt will find all .txt files.")]
            public string Regex = "";
            
            [Wiki("SearchAllDirectories", "If true, all directories will be searched for matching files.")]
            public bool SearchAllDirectories = true;
            
            [Wiki("Recursive", "If true, folders that are not empty will be deleted - otherwise will error.")]
            public bool Recursive = true;

            public Selection()
            {
                
            }

            public Selection(string regex, bool enabled = true, bool recursive = false, bool searchAllDirectories = false)
            {
                Regex = regex;
                Enabled = enabled;
                Recursive = recursive;
                SearchAllDirectories = searchAllDirectories;
            }
            
            public Dictionary<string, object> Serialize()
            {
                return new Dictionary<string, object>
                {
                    ["Enabled"] = Enabled,
                    ["Regex"] = Regex,
                    ["Recursive"] = Recursive,
                    ["SearchAllDirectories"] = SearchAllDirectories
                };
            }

            public void Deserialize(Dictionary<string, object> data)
            {
                Enabled = (bool)data["Enabled"];
                Regex = (string)data["Regex"];
                Recursive = (bool)data["Recursive"];
                SearchAllDirectories = (bool)data["SearchAllDirectories"];
            }
        }

        public enum WhenToExclude
        {
            DoNotCopyFromSource,
            DeleteFromCache,
        }
        
        protected abstract string ListHeader { get; }
        
        [Wiki("WhenToExclude", "If set Source then files matching the regex will not be copied. If set to Cache then files already copied will be removed.")]
        private WhenToExclude m_WhenToExclude = WhenToExclude.DoNotCopyFromSource;
        
        [Wiki("Regexes", "A list of regex to select which files/folders that will be excluded/deleted before being uploaded.")]
        private List<Selection> m_fileRegexes = new List<Selection>();
        
        private ReorderableListOfExcludeFileByRegexSelection m_reorderableList = new ReorderableListOfExcludeFileByRegexSelection();

        public AExcludePathsByRegex_UploadModifier()
        {
            Initialize();
        }

        public AExcludePathsByRegex_UploadModifier(params Selection[] fileRegexes)
        {
            m_fileRegexes.AddRange(fileRegexes);
            Initialize();
        }
        
        public AExcludePathsByRegex_UploadModifier(params string[] fileRegexes)
        {
            foreach (var fileRegex in fileRegexes)
            {
                m_fileRegexes.Add(new Selection(fileRegex));
            }
            Initialize();
        }
        
        public AExcludePathsByRegex_UploadModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false)
        {
            m_fileRegexes.Add(new Selection(fileRegex, true, recursive, searchAllDirectories));
            Initialize();
        }

        private void Initialize()
        {
            m_reorderableList.Initialize(m_fileRegexes, ListHeader, m_fileRegexes.Count <= 2);
        }

        public override void TryGetErrors(UploadConfig config, List<string> errors)
        {
            base.TryGetErrors(config, errors);
            for (var i = 0; i < m_fileRegexes.Count; i++)
            {
                var selection = m_fileRegexes[i];
                if (string.IsNullOrEmpty(selection.Regex))
                {
                    errors.Add($"Regex at index {i+1} is empty");
                }
                else
                {
                    try
                    {
                        Regex.IsMatch("", selection.Regex);
                    }
                    catch (Exception e)
                    {
                        errors.Add("Bad Regex: " + e.Message);
                    }
                }
            }
        }

        public void Add(string path, bool recursive, bool searchAllDirectories)
        {
            m_fileRegexes.Add(new Selection
            {
                Regex = path,
                Recursive = recursive,
                SearchAllDirectories = searchAllDirectories,
            });
        }

        protected abstract string[] GetFiles(string cachedDirectory, Selection regex);
        
        public override bool IgnoreFileDuringCacheSource(string filePath, int configIndex, UploadTaskReport.StepResult stepResult)
        {
            if (m_WhenToExclude != WhenToExclude.DoNotCopyFromSource)
            {
                return false;
            }
            
            for (var i = 0; i < m_fileRegexes.Count; i++)
            {
                var regex = m_fileRegexes[i];
                if (!regex.Enabled)
                {
                    stepResult.AddLog($"Skipping regex {i} because it's disabled");
                    continue;
                }

                if (Regex.IsMatch(filePath, regex.Regex))
                {
                    return true;
                }
            }

            return false;
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedFolderPath, UploadConfig uploadConfig,
            int configIndex, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            if (m_WhenToExclude != WhenToExclude.DeleteFromCache)
            {
                stepResult.AddLog("Skipping " + ListHeader + " modifier because it was done in CacheSources instead");
                return true;
            }
            
            int progressId = ProgressUtils.Start("Exclude Files Modifier", "Removing files from cache...");
            int active = m_fileRegexes.Count(a => a.Enabled);

            bool successful = true;
            int totalExcluded = 0;
            for (var i = 0; i < m_fileRegexes.Count; i++)
            {
                var regex = m_fileRegexes[i];
                if (!regex.Enabled)
                {
                    stepResult.AddLog($"Skipping regex {i} because it's disabled");
                    continue;
                }
            
                await Task.Yield();
            
                float percentDone = (float)i / active;
                ProgressUtils.Report(progressId, percentDone, $"Removing {regex.Regex} files");
            
                try
                {
                    string[] files = GetFiles(cachedFolderPath, regex);
                    foreach (string filePath in files)
                    {
                        totalExcluded++;
                        if (Utils.IsPathADirectory(filePath))
                        {
                            stepResult.AddLog($"Removing directory {filePath} from regex: {regex.Regex} (recursive: {regex.Recursive})");
                            Directory.Delete(filePath, regex.Recursive);
                        }
                        else
                        {
                            stepResult.AddLog($"Removing file {filePath} from regex: {regex.Regex}");
                            File.Delete(filePath);
                        }
                    }
                }
                catch (Exception e)
                {
                    stepResult.AddException(e);
                    stepResult.SetFailed(e.ToString());
                    successful = false;
                }
            }
            
            if (totalExcluded == 0)
            {
                stepResult.AddLog("No files/folders were excluded");
            }

            ProgressUtils.Remove(progressId);
            return successful;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["ExcludedUploadStep"] = m_WhenToExclude.ToString(),
                ["regexes"] = m_fileRegexes.Select(a=>a.Serialize()).ToArray(),
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            if (data == null)
            {
                return;
            }
            
            if (data.TryGetValue("ExcludedUploadStep", out var value))
            {
                string step = (string)value;
                if (Enum.TryParse(step, out WhenToExclude uploadStep))
                {
                    m_WhenToExclude = uploadStep;
                }
                else
                {
                    m_WhenToExclude = WhenToExclude.DoNotCopyFromSource; // Default to Source if parsing fails
                }
            }
            else
            {
                // Migrate old data people expected it to happen to the cache
                m_WhenToExclude = WhenToExclude.DeleteFromCache;
            }
            
            var allRegexes = (List<object>)data["regexes"];
            m_fileRegexes.Clear();
            foreach (object o in allRegexes)
            {
                Selection selection = new Selection();
                selection.Deserialize((Dictionary<string, object>)o);
                m_fileRegexes.Add(selection);
            }
        }
    }
}