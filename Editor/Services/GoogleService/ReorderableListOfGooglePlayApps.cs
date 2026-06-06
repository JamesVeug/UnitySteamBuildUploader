using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGooglePlayApps : InternalReorderableList<GoogleConfig.GooglePlayApp>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GoogleConfig.GooglePlayApp element = list[index];

                // Name
                float labelWidth = 60;
                float textWidth = Mathf.Min(160, rect.width / 3);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this Google Play app. UI only — not sent to Google."));
                rect1.x += rect1.width;

                rect1.width = textWidth;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. My Game (Prod)");
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }
                rect1.x += rect1.width;

                // Padding
                rect1.width = 10;
                GUI.Label(rect1, "");
                rect1.x += rect1.width;

                // Package Name
                rect1.width = labelWidth + 30;
                GUI.Label(rect1, new GUIContent("Package", "Android application ID / package name registered on Google Play."));
                rect1.x += rect1.width;

                rect1.width = rect.width - rect1.x + rect.x - 5;
                string p = EditorUtils.PlaceholderTextField(rect1, element.PackageName, "e.g. com.studio.mygame");
                if (p != element.PackageName)
                {
                    element.PackageName = p;
                    dirty = true;
                }
            }
        }

        protected override GoogleConfig.GooglePlayApp CreateItem(int index)
        {
            return new GoogleConfig.GooglePlayApp(index, "MyPlayApp", "com.example.MyGame");
        }

        protected override int CompareTo(GoogleConfig.GooglePlayApp a, GoogleConfig.GooglePlayApp b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
