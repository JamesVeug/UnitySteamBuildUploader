using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfStrings : InternalReorderableList<string>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string element = list[index];

                Rect rect1 = new Rect(rect.x, rect.y, rect.width, rect.height);
                string n = GUI.TextField(rect1, element);
                if (n != element)
                {
                    list[index] = n.Trim();
                    dirty = true;
                }
            }
        }

        protected override string CreateItem(int index)
        {
            return "";
        }
    }
}