using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_BuildModifier : ABuildConfigModifer
    {
        [Serializable]
        public class Selection
        {
            public bool Enabled = true;
            public string Regex = "";
            public bool Recursive = true;
            public bool SearchAllDirectories = true;

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
        
        protected abstract string ListHeader { get; }
        
        private List<Selection> m_fileRegexes = new List<Selection>();
        private ReorderableListOfExcludeFileByRegexSelection m_reorderableList = new ReorderableListOfExcludeFileByRegexSelection();

        public AExcludePathsByRegex_BuildModifier()
        {
            Initialize();
        }

        public AExcludePathsByRegex_BuildModifier(params Selection[] fileRegexes)
        {
            m_fileRegexes.AddRange(fileRegexes);
            Initialize();
        }
        
        public AExcludePathsByRegex_BuildModifier(params string[] fileRegexes)
        {
            foreach (var fileRegex in fileRegexes)
            {
                m_fileRegexes.Add(new Selection(fileRegex));
            }
            Initialize();
        }
        
        public AExcludePathsByRegex_BuildModifier(string fileRegex, bool recursive = false, bool searchAllDirectories = false)
        {
            m_fileRegexes.Add(new Selection(fileRegex, true, recursive, searchAllDirectories));
            Initialize();
        }

        private void Initialize()
        {
            m_reorderableList.Initialize(m_fileRegexes, ListHeader);
        }

        public override bool IsSetup(out string reason)
        {
            for (var i = 0; i < m_fileRegexes.Count; i++)
            {
                var selection = m_fileRegexes[i];
                if (string.IsNullOrEmpty(selection.Regex))
                {
                    reason = $"Regex at index {i+1} is empty";
                    return false;
                }
                
            }
            
            reason = "";
            return true;
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
        
        public override async Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig,
            int buildIndex, BuildTaskReport.StepResult stepResult)
        {
            int progressId = ProgressUtils.Start("Exclude Files Modifier", "Removing files from cache...");
            int active = m_fileRegexes.Count(a => a.Enabled);

            bool successful = true;
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

                int deleteCount = 0;
                try
                {
                    string[] files = GetFiles(cachedDirectory, regex);
                    foreach (string filePath in files)
                    {
                        deleteCount++;
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

            ProgressUtils.Remove(progressId);
            return successful;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["regexes"] = m_fileRegexes.Select(a=>a.Serialize()).ToArray(),
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
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