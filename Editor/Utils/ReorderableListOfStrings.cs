using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Wireframe
{
    internal class ReorderableListOfStrings
    {
        private class Container : ScriptableObject
        {
            public List<string> strings;
        }

        private Container container;
        private ReorderableList strings_ro_list;
        private SerializedObject serializedObject;
        private SerializedProperty stringsProperty;
        private string header;

        public void Initialize(List<string> list, string listHeader)
        {
            header = listHeader;
            container = ScriptableObject.CreateInstance<Container>();
            container.strings = list;

            serializedObject = new SerializedObject(container);
            stringsProperty = serializedObject.FindProperty("strings");

            strings_ro_list = new ReorderableList(serializedObject, stringsProperty, true, true, true, true);
            strings_ro_list.drawElementCallback = StringsDrawListItems;
            strings_ro_list.drawHeaderCallback = StringsDrawHeader;
        }

        public bool OnGUI()
        {
            if (serializedObject == null)
            {
                return false;
            }

            serializedObject.Update();
            strings_ro_list.DoLayoutList();
            return serializedObject.ApplyModifiedProperties();
        }

        protected virtual void StringsDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            // your GUI code here for list content
            SerializedProperty arrayElementAtIndex = stringsProperty.GetArrayElementAtIndex(index);
            string element = arrayElementAtIndex.stringValue;

            arrayElementAtIndex.stringValue = GUI.TextField(rect, element);
        }

        protected virtual void StringsDrawHeader(Rect rect)
        {
            // your GUI code here for list header
            EditorGUI.LabelField(rect, header);
        }
    }
}