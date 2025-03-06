using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class ExcludeFilesByRegex_BuildModifier : ABuildConfigModifer
    {
        [Serializable]
        public class Selection
        {
            public bool Enabled = true;
            public string Regex = "";
            public bool Recursive = true;
            public bool SearchAllDirectories = true;
            
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
        
        private List<Selection> m_fileRegexes = new List<Selection>();
        private ReorderableListOfExcludeFileByRegexSelection m_reorderableList = new ReorderableListOfExcludeFileByRegexSelection();
        private Action m_onChanged;

        public override void Setup(Action onChanged)
        {
            m_onChanged = onChanged;
            m_reorderableList.Initialize(m_fileRegexes, "Exclude Files", regex =>
            {
                m_onChanged.Invoke();
            });
        }

        public void Add(string path, bool deleteRecursively, bool searchAllDirectories)
        {
            m_fileRegexes.Add(new Selection
            {
                Regex = path,
                Recursive = deleteRecursively,
                SearchAllDirectories = searchAllDirectories
            });
        }
        
        public override async Task<UploadResult> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex)
        {
            int progressId = ProgressUtils.Start("Exclude Files Modifier", "Removing files from cache...");
            int active = m_fileRegexes.Count(a => a.Enabled);

            UploadResult result = UploadResult.Success();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < m_fileRegexes.Count; i++)
            {
                var regex = m_fileRegexes[i];
                if (!regex.Enabled)
                {
                    continue;
                }

                await Task.Yield();

                float percentDone = (float)i / active;
                ProgressUtils.Report(progressId, percentDone, $"Removing {regex.Regex} files");

                int deleteCount = 0;
                try
                {
                    SearchOption searchOption = regex.SearchAllDirectories
                        ? SearchOption.AllDirectories
                        : SearchOption.TopDirectoryOnly;
                    string[] folders = Directory.GetDirectories(cachedDirectory, regex.Regex, searchOption);
                    foreach (string folder in folders)
                    {
                        deleteCount++;
                        sb.AppendLine($"Removing {folder} from regex: {regex.Regex}");
                        if (Utils.IsPathADirectory(folder))
                        {
                            Directory.Delete(folder, regex.Recursive);
                        }
                        else
                        {
                            File.Delete(folder);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to delete files by regex: {regex.Regex} - {e.Message}");
                    result = UploadResult.Failed(e.Message);
                }
                finally
                {
                    if (deleteCount > 0)
                    {
                        sb.Insert(0, "Build #" + buildIndex + "\n");
                        Debug.Log(sb.ToString());
                    }
                }
            }

            ProgressUtils.Remove(progressId);
            return result;
        }

        public override bool OnGUI()
        {
            return m_reorderableList.OnGUI();
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["regexes"] = m_fileRegexes.Select(a=>a.Serialize()).ToArray()
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