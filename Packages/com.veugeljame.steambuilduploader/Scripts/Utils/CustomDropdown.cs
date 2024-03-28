using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract class CustomDropdown<T>
    {
        private const string firstEntryText = "Choose from Dropdown";

        public abstract List<T> GetAllData();
        public abstract string ItemDisplayName(T y);

        private string[] names = null;
        private T[] values = null;

        public void Refresh()
        {
            List<T> data = GetAllData();
            names = new string[data.Count + 1];
            values = new T[data.Count + 1];

            names[0] = firstEntryText;
            values[0] = default(T);
            for (var i = 0; i < data.Count; i++)
            {
                T definition = data[i];
                string name = ItemDisplayName(definition);
                names[i + 1] = name;
            }

            Array.Sort(names, SortNames);
            for (int i = 1; i < names.Length; i++)
            {
                for (var j = 0; j < data.Count; j++)
                {
                    string name = ItemDisplayName(data[j]);
                    if (names[i] == name)
                    {
                        values[i] = data[j];
                        break;
                    }
                }
            }
        }

        private int SortNames(string x, string y)
        {
            if (x == firstEntryText)
            {
                return -1;
            }
            else if (y == firstEntryText)
            {
                return 1;
            }

            return String.Compare(x, y, StringComparison.Ordinal);
        }

        public bool DrawPopup(ref T initial, params GUILayoutOption[] options)
        {
            if (names == null || names.Length == 0 || values == null || values.Length == 0)
            {
                Refresh();
                return false;
            }

            int index = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (initial == null || initial.Equals(values[i]))
                {
                    index = i;
                    break;
                }
            }

            bool edited = false;
            if (index < 0)
            {
                index = 0;
                edited = true;
            }

            int v = EditorGUILayout.Popup(index, names, options);
            if (v != index)
            {
                edited = true;
            }

            T newConfig = values[v];
            initial = newConfig;
            return edited;
        }
    }
}