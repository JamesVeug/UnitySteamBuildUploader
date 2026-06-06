using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfChannels : InternalReorderableList<ItchioChannel>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                ItchioChannel element = list[index];

                float labelWidth = 60;
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Channel", "itch.io channel name. Platform tags like 'windows' or 'osx' set the channel's platform."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. windows-beta");
                if (n != element.Name)
                {
                    element.Name = n.Trim();
                    dirty = true;
                }
            }
        }

        protected override ItchioChannel CreateItem(int index)
        {
            return new ItchioChannel(index, "");
        }
        
        protected override int CompareTo(ItchioChannel a, ItchioChannel b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}