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

                float labelWidth = 55;
                float width = Mathf.Min(150, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Branch", "Name of the branch to publish this build to."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.name, "e.g. master");
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
