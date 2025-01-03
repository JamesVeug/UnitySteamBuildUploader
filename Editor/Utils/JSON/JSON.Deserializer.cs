using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Wireframe
{
    internal static partial class JSON
    {
        internal class JSONDeserializer
        {
            public static T FromJSON<T>(string json)
            {
                return (T)FromJSON(json, typeof(T));
            }

            public static object FromJSON(string json, Type type)
            {
                if (type == typeof(string))
                {
                    return json;
                }

                if (type.IsPrimitive)
                {
                    if (type == typeof(bool))
                    {
                        return bool.Parse(json);
                    }

                    if (type == typeof(int))
                    {
                        return int.Parse(json);
                    }

                    if (type == typeof(float))
                    {
                        return float.Parse(json);
                    }

                    if (type == typeof(double))
                    {
                        return double.Parse(json);
                    }

                    if (type == typeof(long))
                    {
                        return long.Parse(json);
                    }

                    if (type == typeof(short))
                    {
                        return short.Parse(json);
                    }

                    if (type == typeof(byte))
                    {
                        return byte.Parse(json);
                    }

                    if (type == typeof(char))
                    {
                        return char.Parse(json);
                    }

                    if (type == typeof(decimal))
                    {
                        return decimal.Parse(json);
                    }

                    if (type == typeof(uint))
                    {
                        return uint.Parse(json);
                    }

                    if (type == typeof(ulong))
                    {
                        return ulong.Parse(json);
                    }

                    if (type == typeof(ushort))
                    {
                        return ushort.Parse(json);
                    }

                    if (type == typeof(sbyte))
                    {
                        return sbyte.Parse(json);
                    }
                }
                
                if(type.IsEnum)
                {
                    return Enum.Parse(type, json);
                }

                if (type == typeof(object))
                {
                    // Try convert to primitive because we don't know the type
                    if (bool.TryParse(json, out bool boolValue))
                    {
                        return boolValue;
                    }

                    if (long.TryParse(json, out long intValue))
                    {
                        return intValue;
                    }

                    if (float.TryParse(json, out float floatValue))
                    {
                        return floatValue;
                    }

                    if (json[0] == '"' && json[json.Length - 1] == '"')
                    {
                        return json.Substring(1, json.Length - 2);
                    }

                    if (json == "null")
                    {
                        return null;
                    }

                    if (json[0] == '{')
                    {
                        return FromJSON<Dictionary<string, object>>(json);
                    }

                    if (json[0] == '[')
                    {
                        return FromJSON<List<object>>(json);
                    }
                }

                if (type.IsClass)
                {
                    if (type == typeof(string))
                    {
                        return json;
                    }

                    if (type == typeof(DateTime))
                    {
                        return DateTime.Parse(json);
                    }

                    if (type == typeof(DateTimeOffset))
                    {
                        return DateTimeOffset.Parse(json);
                    }

                    if (type == typeof(TimeSpan))
                    {
                        return TimeSpan.Parse(json);
                    }

                    if (type == typeof(Guid))
                    {
                        return Guid.Parse(json);
                    }

                    if (type == typeof(Uri))
                    {
                        return new Uri(json);
                    }

                    if (type == typeof(Version))
                    {
                        return new Version(json);
                    }

                    if (type == typeof(byte[]))
                    {
                        return Convert.FromBase64String(json);
                    }

                    if (type == typeof(bool))
                    {
                        return bool.Parse(json);
                    }

                    if (type == typeof(int))
                    {
                        return int.Parse(json);
                    }

                    // List
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        int startIndex = json.IndexOf("[") + 1;
                        int endIndex = json.LastIndexOf("]");

                        // "value",

                        List<string> listData = new List<string>();
                        int entryStart = startIndex;
                        int entryEnd = startIndex;
                        int depth = 0;
                        bool inString = false;
                        for (int i = startIndex; i < endIndex; i++)
                        {
                            if (json[i] == '{' || json[i] == '[')
                            {
                                depth++;
                            }

                            if (json[i] == '}' || json[i] == ']')
                            {
                                depth--;
                            }

                            if (json[i] == '"')
                            {
                                inString = !inString;
                            }
                            
                            if (json[i] == ',' && depth == 0 && !inString)
                            {
                                entryEnd = i;
                                listData.Add(json.Substring(entryStart, entryEnd - entryStart).Trim());
                                entryStart = i + 1;
                            }
                        }

                        listData.Add(json.Substring(entryStart, endIndex - entryStart).Trim());
                        listData.RemoveAll(string.IsNullOrEmpty);
                        
                        Type listType = type.GetGenericArguments()[0];
                        IList list = (IList)Activator.CreateInstance(type);
                        foreach (string item in listData)
                        {
                            list.Add(FromJSON(item, listType));
                        }

                        return list;
                    }

                    // Dictionary
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        int startIndex = json.IndexOf("{") + 1;
                        int endIndex = json.LastIndexOf("}");

                        // "key": value,
                        // "key": value

                        List<string> entries = new List<string>();
                        int entryStart = startIndex;
                        int entryEnd = startIndex;
                        int depth = 0;
                        bool inString = false;
                        for (int i = startIndex; i < endIndex; i++)
                        {
                            if (json[i] == '{' || json[i] == '[')
                            {
                                depth++;
                            }

                            if (json[i] == '}' || json[i] == ']')
                            {
                                depth--;
                            }

                            if (json[i] == '"')
                            {
                                inString = !inString;
                            }

                            if (json[i] == ',' && depth == 0 && !inString)
                            {
                                entryEnd = i;
                                entries.Add(json.Substring(entryStart, entryEnd - entryStart).Trim());
                                entryStart = i + 1;
                            }
                        }

                        entries.Add(json.Substring(entryStart, endIndex - entryStart).Trim());
                        entries.RemoveAll(string.IsNullOrEmpty);


                        Type keyType = type.GetGenericArguments()[0];
                        Type valueType = type.GetGenericArguments()[1];
                        IDictionary dict = (IDictionary)Activator.CreateInstance(type);

                        foreach (string entry in entries)
                        {
                            int colonIndex = entry.IndexOf(":");
                            string key = entry.Substring(1, colonIndex - 2);
                            string value = entry.Substring(colonIndex + 1).Trim();
                            dict.Add(key, FromJSON(value, valueType));
                        }

                        return dict;
                    }

                    // Class
                    object instance = Activator.CreateInstance(type);
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    Dictionary<string, object> dataDict = FromJSON<Dictionary<string, object>>(json);
                    foreach (FieldInfo field in fields)
                    {
                        string fieldName = field.Name;
                        object fieldValue = dataDict[fieldName];
                        object convertedValue = ConvertType(fieldValue, field.FieldType);
                        // SetField(instance, field, fieldValue);
                        field.SetValue(instance, convertedValue);
                    }

                    return instance;
                }

                Debug.LogError("Type not supported: " + type.Name);
                return null;
            }

            private static object ConvertType(object obj, Type type)
            {
                if (obj == null)
                {
                    return null;
                }

                if (obj.GetType() == type)
                {
                    return obj;
                }

                if (type.IsPrimitive)
                {
                    if (type == typeof(bool))
                    {
                        return bool.Parse(obj.ToString());
                    }

                    if (type == typeof(int))
                    {
                        return int.Parse(obj.ToString());
                    }

                    if (type == typeof(float))
                    {
                        return float.Parse(obj.ToString());
                    }

                    if (type == typeof(double))
                    {
                        return double.Parse(obj.ToString());
                    }

                    if (type == typeof(long))
                    {
                        return long.Parse(obj.ToString());
                    }

                    if (type == typeof(short))
                    {
                        return short.Parse(obj.ToString());
                    }

                    if (type == typeof(byte))
                    {
                        return byte.Parse(obj.ToString());
                    }

                    if (type == typeof(char))
                    {
                        return char.Parse(obj.ToString());
                    }

                    if (type == typeof(decimal))
                    {
                        return decimal.Parse(obj.ToString());
                    }

                    if (type == typeof(uint))
                    {
                        return uint.Parse(obj.ToString());
                    }

                    if (type == typeof(ulong))
                    {
                        return ulong.Parse(obj.ToString());
                    }

                    if (type == typeof(ushort))
                    {
                        return ushort.Parse(obj.ToString());
                    }

                    if (type == typeof(sbyte))
                    {
                        return sbyte.Parse(obj.ToString());
                    }

                    Debug.LogError("Primitive Type not supported: " + type.Name);
                    return null;
                }

                if (type.IsEnum)
                {
                    return Enum.Parse(type, obj.ToString());
                }

                // List
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    IList convertedList = (IList)Activator.CreateInstance(type);
                    Type genericArgument = type.GetGenericArguments()[0];
                    foreach (object item in (List<object>)obj)
                    {
                        convertedList.Add(ConvertType(item, genericArgument));
                    }

                    return convertedList;
                }

                // Dictionary
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    IDictionary convertedDict = (IDictionary)Activator.CreateInstance(type);
                    Type keyType = type.GetGenericArguments()[0];
                    Type valueType = type.GetGenericArguments()[1];
                    foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)obj)
                    {
                        convertedDict.Add(ConvertType(entry.Key, keyType), ConvertType(entry.Value, valueType));
                    }

                    return convertedDict;
                }

                // Class
                if (type.IsClass && obj is Dictionary<string, object> dataDict)
                {
                    object instance = Activator.CreateInstance(type);
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        string fieldName = field.Name;
                        object fieldValue = dataDict[fieldName];
                        field.SetValue(instance, ConvertType(fieldValue, field.FieldType));
                    }

                    return instance;
                }

                Debug.LogError("Type not supported: " + type.Name);
                return null;
            }
        }
    }
}