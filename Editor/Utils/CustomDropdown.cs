using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public interface DropdownElement
    {
        public int Id { get; }
        public string DisplayName { get; }
    }
    
    public abstract class CustomDropdown<T> where T : DropdownElement
    {
        private const string firstEntryText = "Choose from Dropdown";

        public abstract List<T> GetAllData();

        private string[] names = null;
        private T[] values = null;

        public void Refresh()
        {
            List<T> data = new List<T>(GetAllData());
            data.Sort(SortNames);
            
            names = data.ConvertAll(x => x.Id + ". " + x.DisplayName).ToArray();
            values = data.ToArray();
        }

        private static int SortNames(T a, T b)
        {
            if (a.DisplayName == firstEntryText)
            {
                return -1;
            }
            else if (b.DisplayName == firstEntryText)
            {
                return 1;
            }
                
            int compareTo = a.DisplayName.CompareTo(b.DisplayName);
            if (compareTo != 0)
            {
                return compareTo;
            }
            return a.Id.CompareTo(b.Id);
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