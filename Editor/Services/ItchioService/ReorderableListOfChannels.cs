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

                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.Name);
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
    }
}