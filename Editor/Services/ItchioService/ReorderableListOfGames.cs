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

                float labelWidth = 50;
                float width = Mathf.Min(200, rect.width - labelWidth);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Game", "itch.io id for the game, as used by butler (e.g. https://jamesgamesbro.itch.io/my-game. use: 'my-game')."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. my-game");
                if (n != element.Name)
                {
                    element.Name = n.Trim();
                    dirty = true;
                }
            }
        }

        protected override ItchioGameData CreateItem(int index)
        {
            return new ItchioGameData(index, "");
        }
        
        protected override int CompareTo(ItchioGameData a, ItchioGameData b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}