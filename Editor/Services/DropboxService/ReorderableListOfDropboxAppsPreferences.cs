using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDropboxAppsPreferences : InternalReorderableList<DropboxConfig.DropboxApp>
    {
        private bool showToken;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DropboxConfig.DropboxApp element = list[index];

                // Name
                float labelWidth = 50;
                float textWidth = 100;
                Rect rect0 = new Rect(containerRect.x, containerRect.y, labelWidth, containerRect.height);
                GUI.Label(rect0, new GUIContent("Name", "Display name for this Dropbox app. UI only — not sent to Dropbox."));
                rect0.x += rect0.width;

                rect0.width = textWidth;
                string n = EditorUtils.PlaceholderTextField(rect0, element.Name, "e.g. Release Uploader");
                rect0.x += rect0.width;
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }

                // Token
                rect0.width = labelWidth;
                GUI.Label(rect0, new GUIContent("Token", "Dropbox access/refresh token from the App Console."));
                rect0.x += rect0.width;

                rect0.width = containerRect.width - (textWidth * 2) - labelWidth * 2 - 20;
                string dt = element.Token;
                if (showToken)
                {
                    string t = EditorUtils.PlaceholderTextField(rect0, dt, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                    if (t != dt)
                    {
                        element.Token = t;
                        dirty = true;
                    }
                }
                else
                {
                    dt = new string('*', dt.Length);
                    GUI.Label(rect0, dt);
                }
                rect0.x += rect0.width;

                // Padding
                rect0.width = 10;
                GUI.Label(rect0, "");
                rect0.x += rect0.width;

                // Show Token Toggle
                rect0.width = 100;
                showToken = GUI.Toggle(rect0, showToken, "Show");
            }
        }

        protected override DropboxConfig.DropboxApp CreateItem(int index)
        {
            return new DropboxConfig.DropboxApp(index, "MyApp");
        }

        protected override int CompareTo(DropboxConfig.DropboxApp a, DropboxConfig.DropboxApp b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
