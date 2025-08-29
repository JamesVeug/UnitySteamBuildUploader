using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract class CustomDropdown<T> where T : DropdownElement
    {
        public virtual string FirstEntryText => "Choose from Dropdown";
        public virtual bool AddChooseFromDropdownEntry => true;
        public T[] Values
        {
            get
            {
                if(NeedsSettingUp()){
                    Refresh();
                }
                return rawValues;
            }
        }

        protected abstract List<T> FetchAllData();

        private string[] names = null;
        private T[] values = null;
        private T[] rawValues = null;
        private StringFormatter.Context ctx = null;

        public void Refresh()
        {
            List<T> data = new List<T>(FetchAllData());
            data.Sort(SortNames);
            rawValues = data.ToArray();
            
            names = FormatNames();
            
            if (AddChooseFromDropdownEntry)
            {
                data.Insert(0, default);
            }
            values = data.ToArray();
        }

        private string[] FormatNames()
        {
            if (rawValues == null)
            {
                return null; // not setup yet
            }
            
            int count = rawValues.Length + (AddChooseFromDropdownEntry ? 1 : 0);
            List<string> namesTemp = new List<string>(count);
            if (AddChooseFromDropdownEntry)
            {
                namesTemp.Add(FirstEntryText);
            }
            
            if (ctx == null)
            {
                namesTemp.AddRange(rawValues.Select(x => x.Id + ". " + x.DisplayName));
            }
            else
            {
                namesTemp.AddRange(rawValues.Select(x => x.Id + ". " + StringFormatter.FormatString(x.DisplayName, ctx)));
            }
            

            return namesTemp.ToArray();
        }

        private int SortNames(T a, T b)
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

        public bool DrawPopup(IList<T> collection, int index, StringFormatter.Context ctx, params GUILayoutOption[] options)
        {
            T t = collection[index];
            bool edited = DrawPopup(ref t, ctx, options);
            collection[index] = t;
            return edited;
        }
        
        public bool DrawPopup(ref T initial, StringFormatter.Context ctx, params GUILayoutOption[] options)
        {
            if (ctx != this.ctx)
            {
                this.ctx = ctx;
                names = FormatNames();
            }
            
            if (NeedsSettingUp())
            {
                Refresh();
                if (NeedsSettingUp())
                {
                    return false;
                }
            } 

            int index = -1;
            if (!AddChooseFromDropdownEntry && initial == null)
            {
                // Choose first entry in the list
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (initial == null || initial.Equals(values[i]))
                    {
                        index = i;
                        break;
                    }
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

        private bool NeedsSettingUp()
        {
            return names == null || names.Length == 0 || values == null || values.Length == 0;
        }
    }
}