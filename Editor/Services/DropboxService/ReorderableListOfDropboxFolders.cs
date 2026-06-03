using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDropboxFolders : InternalReorderableList<DropboxConfig.DropboxFolder>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DropboxConfig.DropboxFolder element = list[index];

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

                // Path
                rect1.width = 250;
                string newPath = GUI.TextField(rect1, element.Path);
                if (newPath != element.Path)
                {
                    element.Path = newPath;
                    dirty = true;
                }
                rect1.x += rect1.width;
            }
        }

        protected override DropboxConfig.DropboxFolder CreateItem(int index)
        {
            return new DropboxConfig.DropboxFolder(index, "MyDropboxFolder", "");
        }

        protected override int CompareTo(DropboxConfig.DropboxFolder a, DropboxConfig.DropboxFolder b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
