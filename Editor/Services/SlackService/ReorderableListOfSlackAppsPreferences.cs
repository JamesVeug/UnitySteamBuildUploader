using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfSlackAppsPreferences : InternalReorderableList<SlackConfig.SlackApp>
    {
        private bool showToken;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                SlackConfig.SlackApp element = list[index];
                
                // Name
                float labelWidth = 50;
                float textWidth = 100;
                Rect rect0 = new Rect(containerRect.x, containerRect.y, labelWidth, containerRect.height);
                GUI.Label(rect0, new GUIContent("Name", "Display name for this Slack app/bot. UI only — not sent to Slack."));
                rect0.x += rect0.width;

                rect0.width = textWidth;
                string n = EditorUtils.PlaceholderTextField(rect0, element.Name, "e.g. Release Bot");
                rect0.x += rect0.width;
                if (n != element.Name)
                {
                    element.Name = n;
                    dirty = true;
                }

                // Token
                rect0.width = labelWidth;
                GUI.Label(rect0, new GUIContent("Token", "Slack bot token (starts with xoxb-) from api.slack.com -> Your App -> OAuth & Permissions."));
                rect0.x += rect0.width;

                rect0.width = containerRect.width - (textWidth * 2) - labelWidth * 2 - 20; // Adjust width for padding and toggle
                string dt = element.Token;
                if (showToken)
                {
                    string t = EditorUtils.PlaceholderTextField(rect0, dt, "xoxb-xxxxxxxxxxxxx-xxxxxxxxxxxxx-xxxxxxxxxxxxxxxxxxxxxxxx");
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

        protected override SlackConfig.SlackApp CreateItem(int index)
        {
            return new SlackConfig.SlackApp(index, "MyBot");
        }

        protected override int CompareTo(SlackConfig.SlackApp a, SlackConfig.SlackApp b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}