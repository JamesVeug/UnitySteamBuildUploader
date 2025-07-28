using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDiscordAppsPreferences : InternalReorderableList<DiscordConfig.DiscordApp>
    {
        private bool showToken;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DiscordConfig.DiscordApp element = list[index];
                
                // Name
                float labelWidth = 50;
                float textWidth = 100;
                Rect rect0 = new Rect(containerRect.x, containerRect.y, labelWidth, containerRect.height);
                GUI.Label(rect0, "Name");
                rect0.x += rect0.width;

                rect0.width = textWidth;
                string n = GUI.TextField(rect0, element.Name);
                rect0.x += rect0.width;
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }

                // Token
                rect0.width = labelWidth;
                GUI.Label(rect0, "Token");
                rect0.x += rect0.width;
                
                rect0.width = containerRect.width - (textWidth * 2) - labelWidth * 2 - 20; // Adjust width for padding and toggle
                string dt = element.Token;
                if (showToken)
                {
                    string t = GUI.TextField(rect0, dt);
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

        protected override DiscordConfig.DiscordApp CreateItem(int index)
        {
            return new DiscordConfig.DiscordApp(index, "MyBot");
        }
    }
}