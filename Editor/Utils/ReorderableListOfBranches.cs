using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Wireframe
{
    internal class ReorderableListOfBranches
    {
        private class Container : ScriptableObject
        {
            public List<SteamBranch> branches;
        }

        private Container container;
        private ReorderableList list;
        private SerializedObject serializedObject;
        private SerializedProperty branchesProperty;
        private string header = "";
        private Action<SteamBranch> addCallback;

        public void Initialize(List<SteamBranch> listReference, string listHeader,
            Action<SteamBranch> onAddCallback)
        {
            header = listHeader;
            addCallback = onAddCallback;
            container = ScriptableObject.CreateInstance<Container>();
            container.branches = listReference;

            serializedObject = new SerializedObject(container);
            branchesProperty = serializedObject.FindProperty("branches");

            list = new ReorderableList(serializedObject, branchesProperty, true, true, true, true);
            list.drawElementCallback = StringsDrawListItems;
            list.drawHeaderCallback = StringsDrawHeader;
            list.onAddCallback = AddCallback;
        }

        public bool OnGUI()
        {
            if (serializedObject == null)
            {
                return false;
            }

            serializedObject.Update();
            list.DoLayoutList();
            return serializedObject.ApplyModifiedProperties();
        }

        protected virtual void StringsDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            // your GUI code here for list content
            SerializedProperty arrayElementAtIndex = branchesProperty.GetArrayElementAtIndex(index);
            SteamBranch element = (SteamBranch)arrayElementAtIndex.boxedValue;

            Rect rect1 = new Rect(rect.x, rect.y, Mathf.Min(100, rect.width / 2), rect.height);
            string n = GUI.TextField(rect1, element.name);
            if (n != element.name)
            {
                element.name = n;
                arrayElementAtIndex.boxedValue = element;
            }
        }

        protected virtual void StringsDrawHeader(Rect rect)
        {
            // your GUI code here for list header
            EditorGUI.LabelField(rect, header);
        }

        private void AddCallback(ReorderableList l)
        {
            branchesProperty.arraySize++;
            l.index = branchesProperty.arraySize - 1;
            SerializedProperty arrayElementAtIndex = branchesProperty.GetArrayElementAtIndex(l.index);

            SteamBranch depot = new SteamBranch(branchesProperty.arraySize, "");
            arrayElementAtIndex.boxedValue = depot;
            serializedObject.ApplyModifiedProperties();

            addCallback?.Invoke(depot);
        }
    }
}