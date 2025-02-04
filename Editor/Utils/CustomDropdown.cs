﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal interface DropdownElement
    {
        public int Id { get; }
        public string DisplayName { get; }
    }
    
    internal abstract class CustomDropdown<T> where T : DropdownElement
    {
        private const string firstEntryText = "Choose from Dropdown";

        public abstract List<T> GetAllData();

        private string[] names = null;
        private T[] values = null;

        public void Refresh()
        {
            List<T> data = new List<T>(GetAllData());
            data.Sort(SortNames);
            
            List<string> namesTemp = new List<string>(data.Count + 1);
            namesTemp.Add(firstEntryText);
            namesTemp.AddRange(data.ConvertAll(x => x.Id + ". " + x.DisplayName));
            names = namesTemp.ToArray();
            
            data.Insert(0, default);
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
                
            int compareTo = a.Id.CompareTo(b.Id);
            if (compareTo != 0)
            {
                return compareTo;
            }
            return a.DisplayName.CompareTo(b.DisplayName);
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