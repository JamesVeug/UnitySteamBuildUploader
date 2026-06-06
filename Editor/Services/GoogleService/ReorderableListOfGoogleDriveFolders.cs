using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGoogleDriveFolders : InternalReorderableList<GoogleConfig.GoogleDriveFolder>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GoogleConfig.GoogleDriveFolder element = list[index];

                float labelWidth = 50;
                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this folder. UI only — not sent to Google."));
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

                // Folder ID
                rect1.width = 60;
                GUI.Label(rect1, new GUIContent("Folder ID", "Google Drive folder ID — the part after /folders/ in the Drive folder URL."));
                rect1.x += rect1.width;
                rect1.width = 250;
                string newFolderID = EditorUtils.PlaceholderTextField(rect1, element.FolderId, "e.g. 1A2b3C4d (from the Drive folder URL)");
                if (newFolderID != element.FolderId)
                {
                    element.FolderId = newFolderID;
                    dirty = true;
                }
                rect1.x += rect1.width;
            }
        }

        protected override GoogleConfig.GoogleDriveFolder CreateItem(int index)
        {
            return new GoogleConfig.GoogleDriveFolder(index, "MyDriveFolder", "");
        }

        protected override int CompareTo(GoogleConfig.GoogleDriveFolder a, GoogleConfig.GoogleDriveFolder b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
