using System;

namespace Wireframe
{
    /// <summary>
    /// Simple JSON class to serialize and deserialize objects.
    /// Not really efficient, does not work for all cases but at least works for SteamBuildUploader.
    /// Using this instead of a library to avoid dependencies and JSONUtility does not work for what I need.
    /// </summary>
    internal static partial class JSON
    {
        public static string SerializeObject<T>(T data)
        {
            return JSONSerializer.TOJSON(data, typeof(T));
        }
        
        public static string SerializeObject(object data)
        {
            Type type = data == null ? null : data.GetType();
            return JSONSerializer.TOJSON(data, type);
        }

        public static T DeserializeObject<T>(string json)
        {
            return JSONDeserializer.FromJSON<T>(json);
        }
        
        public static object DeserializeObject(string json, Type type)
        {
            return JSONDeserializer.FromJSON(json, type);
        }
    }
}