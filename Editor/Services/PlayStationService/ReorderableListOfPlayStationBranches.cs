using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfPlayStationBranches : InternalReorderableList<PlayStationBranch>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                PlayStationBranch element = list[index];

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

        protected override PlayStationBranch CreateItem(int index)
        {
            return new PlayStationBranch(index, "");
        }

        protected override int CompareTo(PlayStationBranch a, PlayStationBranch b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
