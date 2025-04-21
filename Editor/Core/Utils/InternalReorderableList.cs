using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Wireframe
{
    public abstract class InternalReorderableList<T>
    {
        protected List<T> list;
        protected bool dirty = false;
        
        private ReorderableList reorderableList;
        private string header = "";
        private Action<T> addCallback;

        public void Initialize(List<T> listReference, string listHeader,
            Action<T> onAddCallback)
        {
            header = listHeader;
            addCallback = onAddCallback;
            list = listReference;

            reorderableList = new ReorderableList(listReference, typeof(T), true, true, true, true);
            reorderableList.drawElementCallback = DrawItem;
            reorderableList.drawHeaderCallback = DrawHeader;
            reorderableList.onAddCallback = AddCallback;
            reorderableList.onRemoveCallback = RemoveCallback;
            reorderableList.onChangedCallback = ChangedCallback;
        }

        public bool OnGUI()
        {
            reorderableList.DoLayoutList();
            if (dirty)
            {
                dirty = false;
                return true;
            }
            return false;
        }

        protected abstract void DrawItem(Rect rect, int index, bool isActive, bool isFocused);
        protected abstract T CreateItem(int index);

        protected virtual void DrawHeader(Rect rect)
        {
            // your GUI code here for list header
            EditorGUI.LabelField(rect, header);
        }

        private void AddCallback(ReorderableList l)
        {
            l.index++;
            T depot = CreateItem(list.Count + 1);
            list.Add(depot);
            dirty = true;

            addCallback?.Invoke(depot); 
        }
        
        private void RemoveCallback(ReorderableList list1)
        {
            list.RemoveAt(list1.index);
            dirty = true;
        }

        private void ChangedCallback(ReorderableList list1)
        {
            dirty = true;
        }
    }
}