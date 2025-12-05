using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Wireframe
{
    public static partial class JSON
    {
        private class JSONSerializer
        {
            public static string TOJSON(object o, Type type, int indents = 0)
            {
                if (o == null)
                {
                    return "null";
                }
                
                if (type == typeof(string))
                {
                    string s = (string)o;
                    s = s.Replace("\r", "\\r");
                    s = s.Replace("\n", "\\n");
                    s = s.Replace("\"", "\\\"");
                    return "\"" + s + "\"";
                }
                if (type.IsPrimitive)
                {
                    return o.ToString().ToLowerInvariant();
                }

                if (type.IsEnum)
                {
                    return ((int)o).ToString();
                }
                
                // List
                if(o is IList list)
                {
                    StringBuilder sb = new StringBuilder();
                    indents++;
                    sb.Append("[");
                    sb.Append("\n");
                    sb.Append(new string('\t', indents));
                    
                    for (var i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        sb.Append(TOJSON(item, item.GetType(), indents));
                        if (i < list.Count - 1)
                        {
                            sb.Append(",");
                            sb.Append("\n");
                            sb.Append(new string('\t', indents));
                        }
                    }

                    indents--;
                    sb.Append("\n");
                    sb.Append(new string('\t', indents));
                    sb.Append("]");
                    return sb.ToString();
                }
                
                // Dictionary
                if(o is IDictionary dict)
                {
                    indents++;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append("\n");
                    sb.Append(new string('\t', indents));
                    int count = dict.Count;
                    foreach (DictionaryEntry entry in dict)
                    {
                        sb.Append(TOJSON(entry.Key, entry.Key?.GetType(), indents));
                        sb.Append(": ");
                        sb.Append(TOJSON(entry.Value, entry.Value?.GetType(), indents));
                        if (--count > 0)
                        {
                            sb.Append(",");
                            sb.Append("\n");
                            sb.Append(new string('\t', indents));
                        }
                    }
                    sb.Append("\n");
                    sb.Append(new string('\t', Mathf.Max(0, indents - 1)));
                    sb.Append("}");
                    indents--;
                    return sb.ToString();
                }

                if (type.IsClass)
                {
                    indents++;
                    
                    // Get all serializable fields
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    Dictionary<string, string> dataDict = new Dictionary<string, string>();
                    foreach (FieldInfo field in fields)
                    {
                        string fieldName = field.Name;
                        object fieldValue = field.GetValue(o);
                        string serializedField = TOJSON(fieldValue, field.FieldType, indents);
                        dataDict.Add(fieldName, serializedField);
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append("\n");
                    sb.Append(new string('\t', indents));
                    int count = dataDict.Count;
                    foreach (KeyValuePair<string, string> entry in dataDict)
                    {
                        sb.Append("\"" + entry.Key + "\": ");
                        sb.Append(entry.Value);
                        if (--count > 0)
                        {
                            sb.Append(",");
                            sb.Append("\n");
                            sb.Append(new string('\t', indents));
                        }
                    }
                    indents--;
                    sb.Append("\n");
                    sb.Append(new string('\t', indents));
                    sb.Append("}");
                    return sb.ToString();
                }
                
                return null;
            }
        }
    }
}