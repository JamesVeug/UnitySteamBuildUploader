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
        
        [MenuItem("Window/Build Uploader/Export Wiki Data", false, 20)]
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
            allData.Add(new Data()
            {
                DataClass = typeof(BuildConfig.ModifierData),
                MDFilePath = Path.Combine(Application.dataPath, "../Wiki/Modifiers.md"),
                StartOfHeader = "## Modifiers",
                WikiSubPath = "modifiers",
            });
            allData.Add(new Data()
            {
                DataClass = typeof(BuildConfig.DestinationData),
                MDFilePath = Path.Combine(Application.dataPath, "../Wiki/Destinations.md"),
                StartOfHeader = "## Destinations",
                WikiSubPath = "destinations",
            });
            
            
            // Get every type matching the WikiSubPath
            var types = typeof(Wiki).Assembly
                .GetTypes()
                .Where(t => t.IsDefined(typeof(WikiAttribute)))
                .ToList();
            types.Sort(SortTypesByWikiAttribute);
            
            foreach (var type in types)
            {
                var wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(type, typeof(WikiAttribute));
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

                while (text[startIndex] == '\n' || text[startIndex] == '\r')
                {
                    startIndex--;
                }
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(text.Substring(0, startIndex));
                sb.AppendLine();
                sb.AppendLine();
                

                WikiAttribute dataWikiAttribute = (WikiAttribute)data.DataClass.GetCustomAttribute(typeof(WikiAttribute));
                sb.AppendLine($"## {dataWikiAttribute.Name}");
                if(!string.IsNullOrEmpty(dataWikiAttribute.Text))sb.AppendLine($"{dataWikiAttribute.Text}");
                WriteFields(data.DataClass, sb, 1);
                sb.AppendLine();
                

                foreach (Type type in data.Types)
                {
                    WriteTypeData(type, sb, 3);
                }
                
                File.WriteAllText(mdFilePath, sb.ToString());
            }
        }

        private static int SortTypesByWikiAttribute(Type a, Type b)
        {
            WikiAttribute aW = (WikiAttribute)a.GetCustomAttribute(typeof(WikiAttribute));
            WikiAttribute bW = (WikiAttribute)b.GetCustomAttribute(typeof(WikiAttribute));
            if (aW.Order != bW.Order)
            {
                return aW.Order - bW.Order;
            }

            return string.Compare(aW.Name, bW.Name, StringComparison.Ordinal);
        }

        private static void WriteTypeData(Type type, StringBuilder sb, int headerIndent)
        {
            var wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(type, typeof(WikiAttribute));
            sb.AppendLine(new string('#', headerIndent) + " " + wikiAttribute.Name);
            sb.AppendLine(wikiAttribute.Text);
            
            WriteFields(type, sb, 0);
            sb.AppendLine();
        }

        private static void WriteFields(Type type, StringBuilder sb, int headerIndent)
        {
            var fields = ReflectionUtils.GetAllFields(type)
                .Where(a=> a.IsDefined(typeof(WikiAttribute)))
                .OrderBy(a=>a.Name)
                .ToList();
            fields.Sort((a,b)=>
            {
                WikiAttribute aW = (WikiAttribute)a.GetCustomAttribute(typeof(WikiAttribute));
                WikiAttribute bW = (WikiAttribute)b.GetCustomAttribute(typeof(WikiAttribute));
                if (aW.Order != bW.Order)
                {
                    return aW.Order - bW.Order;
                }

                return string.Compare(aW.Name, bW.Name, StringComparison.Ordinal);
            });

            foreach (FieldInfo field in fields)
            {
                WikiAttribute wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(field, typeof(WikiAttribute));
                sb.AppendLine($"{new string(' ', headerIndent*2)}- **{wikiAttribute.Name}**: {wikiAttribute.Text}");
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
                else if (field.FieldType.GetCustomAttribute(typeof(WikiAttribute)) != null)
                {
                    WriteFields(field.FieldType, sb, headerIndent + 1);
                }
                else if (field.FieldType.GenericTypeArguments.Length > 0 && field.FieldType.GenericTypeArguments[0].GetCustomAttribute(typeof(WikiAttribute)) != null)
                {
                    WriteFields(field.FieldType.GenericTypeArguments[0], sb, headerIndent + 1);
                }
            }
        }
    }
}
#endif
