using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGames : InternalReorderableList<ItchioGameData>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                ItchioGameData element = list[index];

                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.Name);
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }
            }
        }

        protected override ItchioGameData CreateItem(int index)
        {
            return new ItchioGameData(index, "");
        }
    }
}