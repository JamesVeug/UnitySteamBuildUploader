#if BUILD_UPLOADER_WIKI
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class Wiki
    {
        private class Data
        {
            public Type DataClass;
            public string MDFilePath; // Path to the markdown file in your unity project (same level as Assets)
            public string StartOfHeader; // Search for this to start inserting the generated text
            public string WikiSubPath; // Url where this goes to
            public List<Type> Types = new List<Type>();
        }
        
        [MenuItem("Window/Build Uploader/Export Wiki Data")]
        public static void ExportWikiData()
        {
            List<Data> allData = new List<Data>();
            allData.Add(new Data()
            {
                DataClass = typeof(BuildConfig.SourceData),
                MDFilePath = Path.Combine(Application.dataPath, "../Wiki/Sources.md"),
                StartOfHeader = "## Sources",
                WikiSubPath = "sources",
            });
            
            
            // Get every type matching the WikiSubPath
            var types = typeof(Wiki).Assembly.GetTypes();
            foreach (var type in types)
            {
                var wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(type, typeof(WikiAttribute));
                if (wikiAttribute == null)
                {
                    continue;
                }

                Data d = allData.FirstOrDefault(a => a.WikiSubPath == wikiAttribute.SubPath);
                if (d == null)
                {
                    if (!string.IsNullOrEmpty(wikiAttribute.SubPath))
                    {
                        Debug.LogError("Could not find data for: " + wikiAttribute.SubPath);
                    }

                    continue;
                }
                
                d.Types.Add(type);
                Debug.Log($"Type: {type.Name}, Wiki Link: {wikiAttribute.Text}");
            }

            // Write each data type (source,modifier,destination)
            foreach (Data data in allData)
            {
                string mdFilePath = data.MDFilePath;
                string text = File.ReadAllText(mdFilePath);
                
                // Find the start of the header
                int startIndex = text.IndexOf(data.StartOfHeader);
                if (startIndex == -1)
                {
                    Debug.LogError($"Could not find header: {data.StartOfHeader} in {mdFilePath}");
                    continue;
                }
                
                // Find the end of the header
                int endIndex = startIndex + data.StartOfHeader.Length;
                if (endIndex == -1)
                {
                    Debug.LogError($"Could not find end of header: {data.StartOfHeader} in {mdFilePath}");
                    continue;
                }
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(text.Substring(0, startIndex));

                WikiAttribute dataWikiAttribute = (WikiAttribute)data.DataClass.GetCustomAttribute(typeof(WikiAttribute));
                sb.AppendLine($"## {dataWikiAttribute.Name}");
                if(!string.IsNullOrEmpty(dataWikiAttribute.Text))sb.AppendLine($"{dataWikiAttribute.Text}");
                WriteFields(data.DataClass, sb);
                sb.AppendLine();
                

                foreach (Type type in data.Types)
                {
                    WriteTypeData(type, sb, 3);
                }
                
                File.WriteAllText(mdFilePath, sb.ToString());
            }
        }

        private static void WriteTypeData(Type type, StringBuilder sb, int headerIndent)
        {
            var wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(type, typeof(WikiAttribute));
            sb.AppendLine(new string('#', headerIndent) + " " + wikiAttribute.Name);
            sb.AppendLine(wikiAttribute.Text);

            foreach (var VARIABLE in type.GetNestedTypes().Where(a=> a.IsDefined(typeof(WikiAttribute))))
            {
                WriteTypeData(VARIABLE, sb, headerIndent + 1);
            }
            
            WriteFields(type, sb);
            sb.AppendLine();
        }

        private static void WriteFields(Type type, StringBuilder sb)
        {
            var fields = ReflectionUtils.GetAllFields(type)
                .Where(a=> a.IsDefined(typeof(WikiAttribute)))
                .OrderBy(a=>a.Name)
                .ToArray();

            foreach (FieldInfo field in fields)
            {
                WikiAttribute wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(field, typeof(WikiAttribute));
                sb.AppendLine($"- **{wikiAttribute.Name}**: {wikiAttribute.Text}");
                if (field.FieldType.IsEnum)
                {
                    foreach (object e in Enum.GetValues(field.FieldType))
                    {
                        WikiAttribute enumWikiAttribute = ((Enum)e).GetAttributeOfType<WikiAttribute>();
                        string enumName = e.ToString();
                        if (enumWikiAttribute != null)
                        {
                            enumName = $"{enumWikiAttribute.Name}: {enumWikiAttribute.Text}";
                        }
                        sb.AppendLine($"  - {enumName}");
                    }
                }
            }
        }
    }
}
#endif
