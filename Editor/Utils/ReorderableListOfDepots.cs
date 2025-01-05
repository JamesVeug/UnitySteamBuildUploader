using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Wireframe;

namespace Wireframe
{
    internal class ReorderableListOfDepots
    {
        private class Container : ScriptableObject
        {
            public List<SteamBuildDepot> depots;
        }

        private Container container;
        private ReorderableList list;
        private SerializedObject serializedObject;
        private SerializedProperty depotsProperty;
        private string header = "";
        private Action<SteamBuildDepot> addCallback;

        public void Initialize(List<SteamBuildDepot> listReference, string listHeader,
            Action<SteamBuildDepot> onAddCallback)
        {
            header = listHeader;
            addCallback = onAddCallback;
            container = ScriptableObject.CreateInstance<Container>();
            container.depots = listReference;

            serializedObject = new SerializedObject(container);
            depotsProperty = serializedObject.FindProperty("depots");

            list = new ReorderableList(serializedObject, depotsProperty, true, true, true, true);
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
            SerializedProperty arrayElementAtIndex = depotsProperty.GetArrayElementAtIndex(index);
            SteamBuildDepot element = (SteamBuildDepot)arrayElementAtIndex.boxedValue;

            Rect rect1 = new Rect(rect.x, rect.y, Mathf.Min(100, rect.width / 2), rect.height);
            string n = GUI.TextField(rect1, element.Name);
            if (n != element.Name)
            {
                element.Name = n;
                arrayElementAtIndex.boxedValue = element;
            }

            rect1.x += rect1.width;
            string textField = GUI.TextField(rect1, element.Depot.DepotID.ToString());
            if (int.TryParse(textField, out int value) && value != element.Depot.DepotID)
            {
                element.Depot.DepotID = value;
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
            depotsProperty.arraySize++;
            l.index = depotsProperty.arraySize - 1;
            SerializedProperty arrayElementAtIndex = depotsProperty.GetArrayElementAtIndex(l.index);

            SteamBuildDepot buildDepot = new SteamBuildDepot(depotsProperty.arraySize, "");
            arrayElementAtIndex.boxedValue = buildDepot;
            serializedObject.ApplyModifiedProperties();

            addCallback?.Invoke(buildDepot);
        }
    }
}