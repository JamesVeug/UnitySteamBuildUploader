using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGoogleChatSpacesProjectSettings : InternalReorderableList<GoogleConfig.GoogleChatSpace>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GoogleConfig.GoogleChatSpace element = list[index];

                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.Name);
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }

                // Padding
                rect1.x += rect1.width;
                rect1.width = 10;
                GUI.Label(rect1, "");
                rect1.x += rect1.width;
            }
        }

        protected override GoogleConfig.GoogleChatSpace CreateItem(int index)
        {
            return new GoogleConfig.GoogleChatSpace(index, "MyChatSpace");
        }

        protected override int CompareTo(GoogleConfig.GoogleChatSpace a, GoogleConfig.GoogleChatSpace b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
