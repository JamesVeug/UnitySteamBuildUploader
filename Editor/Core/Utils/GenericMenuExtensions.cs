using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class GenericMenuExtensions
    {
        public static void AddMenuItem(this GenericMenu menu, string text, bool on, GenericMenu.MenuFunction action, bool disabled=false)
        {
            if (disabled)
            {
                menu.AddDisabledItem(new GUIContent(text));
            }
            else
            {
                menu.AddItem(new GUIContent(text), on, action);
            }
        }
    }
}