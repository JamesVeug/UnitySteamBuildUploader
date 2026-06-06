using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGoogleAppsProjectSettings : InternalReorderableList<GoogleConfig.GoogleApp>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GoogleConfig.GoogleApp element = list[index];

                float labelWidth = 50;
                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this Google app. UI only — not sent to Google."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. Release Uploader");
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

        protected override GoogleConfig.GoogleApp CreateItem(int index)
        {
            return new GoogleConfig.GoogleApp(index, "MyApp");
        }

        protected override int CompareTo(GoogleConfig.GoogleApp a, GoogleConfig.GoogleApp b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
