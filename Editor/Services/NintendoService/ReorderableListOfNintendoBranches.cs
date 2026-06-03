using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfNintendoBranches : InternalReorderableList<NintendoBranch>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                NintendoBranch element = list[index];

                float width = Mathf.Min(150, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.name);
                if (n != element.name)
                {
                    element.name = n;
                    dirty = true;
                }
            }
        }

        protected override NintendoBranch CreateItem(int index)
        {
            return new NintendoBranch(index, "");
        }

        protected override int CompareTo(NintendoBranch a, NintendoBranch b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
