﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract class CustomMultiDropdown<T, Y> where Y : DropdownElement
    {
        public virtual string FirstEntryText => "Choose from Dropdown";

        public abstract List<(T, List<Y>)> GetAllData();

        private static Dictionary<T, string[]> nameLookup;
        private static Dictionary<T, Y[]> valueLookup;

        public virtual bool IsItemValid(Y y)
        {
            return true;
        }

        public bool DrawPopup(T target, ref Y initial, params GUILayoutOption[] options)
        {
            if (FailedToRefresh())
            {
                return false;
            }
            else if (target == null)
            {
                return false;
            }
            else if (!valueLookup.ContainsKey(target))
            {
                return false;
            }

            Y[] values = valueLookup[target];

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

            string[] names = nameLookup[target];
            if (names.Length == 0)
                return false;

            int v = EditorGUILayout.Popup(index, names, options);
            if (v != index)
            {
                edited = true;
            }

            Y newConfig = values[v];
            initial = newConfig;
            return edited;
        }

        public void Refresh()
        {
            nameLookup = new Dictionary<T, string[]>();
            valueLookup = new Dictionary<T, Y[]>();

            List<(T, List<Y>)> currentBuilds = GetAllData();
            if (currentBuilds == null)
                return;

            for (int i = 0; i < currentBuilds.Count; i++)
            {
                List<Y> builds = new List<Y>(currentBuilds[i].Item2);
                builds.Sort(SortByName);

                List<string> names = new List<string>();
                List<Y> values = new List<Y>();

                names.Add(FirstEntryText);
                values.Add(default);

                for (var j = 0; j < builds.Count; j++)
                {
                    if (IsItemValid(builds[j]))
                    {
                        values.Add(builds[j]);
                        names.Add(builds[j].Id + ". " + builds[j].DisplayName);
                    }
                }
                
                nameLookup[currentBuilds[i].Item1] = names.ToArray();
                valueLookup[currentBuilds[i].Item1] = values.ToArray();
            }
        }

        public virtual int SortByName(Y a, Y b)
        {
            if (a.DisplayName == FirstEntryText)
            {
                return -1;
            }
            else if (b.DisplayName == FirstEntryText)
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

        private bool FailedToRefresh()
        {
            if (NeedsRefreshing())
            {
                Refresh();
                if (NeedsRefreshing())
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool NeedsRefreshing()
        {
            return nameLookup == null || nameLookup.Count == 0 || valueLookup == null || valueLookup.Count == 0;
        }
    }
}