using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Wireframe
{
    [Serializable]
   public abstract class VDFFile
    {
        public abstract string FileName { get; }


        public static async Task<bool> Save<T>(T t, string path) where T : VDFFile, new()
        {
            string content = ConvertToString(t, "\"" + t.FileName + "\"", "");
            if (!File.Exists(path))
            {
                FileStream stream = File.Create(path);
#if UNITY_2021_2_OR_NEWER
                await stream.DisposeAsync();
#else
                stream.Dispose();
#endif
            }

            Debug.Log("Writing content to: " + path);
            try
            {
#if UNITY_2021_2_OR_NEWER
                await File.WriteAllTextAsync(path, content);
#else
                File.WriteAllText(path, content);
#endif
                Debug.Log("Saved VDFFile to: " + path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to write to file: " + e.Message);
                return false;
            }
        }

        private static string ConvertToString(object data, string dataLabel, string indent)
        {
            string content = "";
            Type dataType = data?.GetType();
            if (data == null)
            {
                content = "\"\"";
            }
            else if (dataType == typeof(bool))
            {
                if ((bool)data == true)
                {
                    content = "\"1\"";
                }
                else
                {
                    content = "\"0\"";
                }
            }
            else if (dataType.IsPrimitive || dataType == typeof(Decimal) || dataType == typeof(String))
            {
                content = string.Format("\"{0}\"", data);
            }
            else if (dataType.IsSubclassOf(typeof(VDFFile)))
            {
                // TODO: Change Depot to not be a file or it will draw the name twice.
                // File
                content = "\n{";

                // Save content
                FieldInfo[] fields = dataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; i++)
                {
                    string fieldName = "\"" + fields[i].Name + "\"";
                    object fieldData = fields[i].GetValue(data);
                    string fieldContent = ConvertToString(fieldData, fieldName, indent + "\t");

                    content += String.Format("\n{0}", fieldContent);
                }

                content += "\n}";
            }
            else if (dataType.GetInterfaces().Contains(typeof(IVdfMap)))
            {
                // Maps of data
                content = "\n{";

                MethodInfo getKeyMethod = dataType.GetMethod("GetKey");
                MethodInfo getValueMethod = dataType.GetMethod("GetValue");

                int count = (int)dataType.GetProperty("Count").GetValue(data, null);
                for (int i = 0; i < count; i++)
                {
                    object key = getKeyMethod.Invoke(data, new object[] { i });
                    object value = getValueMethod.Invoke(data, new object[] { i });

                    string keyString = ConvertToString(key, "", "");
                    string valueString = ConvertToString(value, keyString, "");

                    content += "\n\t" + valueString;
                }

                content += "\n}";
            }
            else
            {
                content = "\"\"";
            }

            content = content.Replace("\n", "\n" + indent);
            if (dataLabel.Length == 0)
            {
                return content;
            }
            else
            {
                return string.Format(indent + "{0} {1}", dataLabel, content);
            }
        }

        public static T Load<T>(string path) where T : VDFFile, new()
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Cannot find '" + path + "'");
                return default(T);
            }

            // create new object
            T t = new T();

            // read from file
            string[] lines = File.ReadAllLines(path);

            // parse lines
            List<List<string>> parsedLines = new List<List<string>>();
            Regex regex = new Regex("\"[^\"]*\"", RegexOptions.IgnorePatternWhitespace);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                List<string> words = new List<string>();
                if (line == "{" || line == "}")
                {
                    words.Add(line);
                }
                else
                {
                    var matches = regex.Matches(line);
                    foreach (Match match in matches)
                    {
                        words.Add(match.ToString());
                    }
                }

                parsedLines.Add(words);
            }

            int index = 0;

            Parse(t, typeof(T), parsedLines, ref index);
            return t;
        }

        private static void Parse(object data, Type dataType, List<List<string>> lines, ref int index)
        {
            index++; // "appbuild"   -- skip this

            // get where this file ends
            int endIndex = GetEndBracketIndex(lines, index);
            if (endIndex < 0)
            {
                Debug.LogError("Missing closing curly bracket!");
                return;
            }

            // Skip { and }
            while (++index < endIndex)
            {
                List<string> words = lines[index];
                if (words.Count == 0 || string.IsNullOrEmpty(words[0]))
                {
                    // Empty line
                    continue;
                }
                else if (words[0][0] == '/' && words[0][1] == '/')
                {
                    // Comment
                    continue;
                }


                if (words.Count == 1)
                {
                    // Inner VDF:
                    /*"depots"
                    {
                        "x" "depot_build_x.vdf"
                    }*/
                    string variableName = words[0].Substring(1, words[0].Length - 2);
                    FieldInfo myFieldInfo =
                        dataType.GetField(variableName, BindingFlags.Public | BindingFlags.Instance);
                    if (myFieldInfo == null)
                    {
                        Debug.LogError("[VDF] Could not find field name: '" + variableName + "'");
                        continue;
                    }

                    if (myFieldInfo.FieldType.IsSubclassOf(typeof(VDFFile)))
                    {
                        object o = myFieldInfo.GetValue(data);
                        Parse(o, myFieldInfo.FieldType, lines, ref index);
                    }
                    else if (myFieldInfo.FieldType.GetInterfaces().Contains(typeof(IVdfMap)))
                    {
                        // Map of data - TODO:
                        IVdfMap map = ParseMap(myFieldInfo.FieldType, lines, ref index);
                        myFieldInfo.SetValue(data, map);
                    }
                    else
                    {
                        Debug.LogError("[VDF] Unknown type: '" + myFieldInfo.FieldType + "'" + "'" + variableName +
                                       "'");
                        index = endIndex;
                        continue;
                    }
                }
                else if (words.Count == 2)
                {
                    // Field: "appid"	"x"
                    string variableName = words[0].Substring(1, words[0].Length - 2);
                    string variableData = words[1].Substring(1, words[1].Length - 2);
                    SetField(data, dataType, variableName, variableData);
                }
                else
                {
                    throw new NotImplementedException("Case not covered with '" + words.Count + "' length.");
                }
            }
        }

        private static IVdfMap ParseMap(Type fieldType, List<List<string>> lines, ref int index)
        {
            // "depots"
            // {
            //     "x" "depot_build_x.vdf"
            // }
            IVdfMap map = (IVdfMap)Activator.CreateInstance(fieldType);

            MethodInfo methodInfo = fieldType.GetMethod("Add");
            Type keyType;
            Type valueType;
            if (fieldType.GetGenericArguments().Length == 0)
            {
                keyType = fieldType.BaseType.GetGenericArguments()[0];
                valueType = fieldType.BaseType.GetGenericArguments()[1];
            }
            else
            {
                keyType = fieldType.GetGenericArguments()[0];
                valueType = fieldType.GetGenericArguments()[1];
            }

            int startIndex = ++index;
            int endIndex = GetEndBracketIndex(lines, startIndex);
            if (endIndex < 0)
            {
                throw new Exception("Missing closing curly bracket for map!");
            }

            while (++index < endIndex)
            {
                List<string> words = lines[index];
                if (words.Count == 2)
                {
                    // Field: "appid"	"x"
                    string key = words[0].Substring(1, words[0].Length - 2);
                    string value = words[1].Substring(1, words[1].Length - 2);

                    object convertedKey = ConvertToType(key, keyType);
                    object convertedValue = ConvertToType(value, valueType);

                    object[] a = new[] { convertedKey, convertedValue };
                    methodInfo.Invoke(map, a);
                }
                else
                {
                    throw new Exception("Malformed map");
                }
            }

            return map;
        }

        private static void SetField(object t, Type type, string variableName, string variableData)
        {
            FieldInfo myFieldInfo = type.GetField(variableName, BindingFlags.Public | BindingFlags.Instance);
            if (myFieldInfo == null)
            {
                Debug.LogError("[VDF] Could not find field name: '" + variableName + "'");
                return;
            }

            object o;
            if (variableData.Length > 0)
            {
                o = ConvertToType(variableData, myFieldInfo.FieldType);
            }
            else
            {
                ConstructorInfo ctor = myFieldInfo.FieldType.GetConstructor(new Type[0]);
                if (ctor != null)
                {
                    o = ctor.Invoke(null);
                }
                else
                {
                    o = default;

                }
            }

            myFieldInfo.SetValue(t, o);
            Debug.Log("[VDF] Set: '" + variableName + "' to: '" + variableData + "'");
        }

        private static object ConvertToType(string variableData, Type type)
        {
            if (type == typeof(bool))
            {
                int i;
                if (int.TryParse(variableData, out i))
                {
                    bool b = i == 1;
                    return b;
                }
            }

            object o = Convert.ChangeType(variableData, type);
            return o;
        }

        private static int GetEndBracketIndex(List<List<string>> list, int startIndex)
        {
            int brackets = 0;
            for (int i = startIndex; i < list.Count; i++)
            {
                List<string> line = list[i];
                if (line.Count != 1)
                {
                    continue;
                }

                string text = line[0];
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                else if (text[0] == '{')
                {
                    brackets++;
                }
                else if (text[0] == '}')
                {
                    brackets--;
                    if (brackets == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}