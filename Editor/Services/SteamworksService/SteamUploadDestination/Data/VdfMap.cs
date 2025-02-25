using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    internal abstract class VdfMap<T, Y> : IVdfMap
    {
        internal class MapData
        {
            public T Key;
            public Y Value;

            public MapData(T key, Y value)
            {
                Key = key;
                Value = value;
            }
        }

        public int Count
        {
            get { return Data.Count; }
        }

        public List<MapData> GetData()
        {
            return Data;
        }

        private List<MapData> Data;

        public VdfMap()
        {
            Data = new List<MapData>();
        }

        public void Add(T key, Y value)
        {
            Data.Add(new MapData(key, value));
        }

        public void RemoveAt(int i)
        {
            Data.RemoveAt(i);
        }

        public T GetKey(int i)
        {
            return Data[i].Key;
        }

        public Y GetValue(int i)
        {
            return Data[i].Value;
        }

        public void Clear()
        {
            Data.Clear();
        }
    }
}