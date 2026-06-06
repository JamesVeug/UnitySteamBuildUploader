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

                float labelWidth = 50;
                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this folder. UI only — not sent to Dropbox."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. Nightly Builds");
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
                rect1.width = 40;
                GUI.Label(rect1, new GUIContent("Path", "Destination folder path in Dropbox. Created if it does not exist."));
                rect1.x += rect1.width;
                rect1.width = 250;
                string newPath = EditorUtils.PlaceholderTextField(rect1, element.Path, "e.g. /Builds/Android");
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
