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

        private bool m_showFolderOut;
        private ReorderableList reorderableList;
        private string header = "";
        private Action<T> addCallback;

        public void Initialize(List<T> listReference, string listHeader, Action<T> onAddCallback=null)
        {
            header = listHeader;
            addCallback = onAddCallback;
            list = listReference;

            reorderableList = new ReorderableList(listReference, typeof(T), true, true, true, true);
            reorderableList.headerHeight = 0;
            reorderableList.drawElementCallback = DrawItem;
            reorderableList.onAddCallback = AddCallback;
            reorderableList.onRemoveCallback = RemoveCallback;
            reorderableList.onChangedCallback = ChangedCallback;
        }

        public bool OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope("RL Header"))
            {
                GUILayout.Space(5);
                string headerText = $"{header ?? ""} ({list.Count})";
                m_showFolderOut = EditorGUILayout.Foldout(m_showFolderOut, headerText);
            }

            if (m_showFolderOut)
            {
                reorderableList.DoLayoutList();
            }

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
            if (!string.IsNullOrEmpty(header))
            {
                EditorGUI.LabelField(rect, header);
            }
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
        
        public void SetHeaderText(string newHeader)
        {
            header = newHeader;
        }
    }
}