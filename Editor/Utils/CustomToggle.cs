using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class CustomToggle
    {
        public static bool DrawToggle(string label, ref bool value, params GUILayoutOption[] options)
        {
            bool newValue = EditorGUILayout.Toggle(label, value, options);
            if (newValue != value)
            {
                value = newValue;
                return true;
            }

            return false;
        }
        public static bool DrawToggle(ref bool value, params GUILayoutOption[] options)
        {
            bool newValue = EditorGUILayout.Toggle(value, options);
            if (newValue != value)
            {
                value = newValue;
                return true;
            }

            return false;
        }
    }
}