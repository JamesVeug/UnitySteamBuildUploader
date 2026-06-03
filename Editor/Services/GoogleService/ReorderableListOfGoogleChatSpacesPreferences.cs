using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfGoogleChatSpacesPreferences : InternalReorderableList<GoogleConfig.GoogleChatSpace>
    {
        private bool showWebhook;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GoogleConfig.GoogleChatSpace element = list[index];

                // Name
                float labelWidth = 70;
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

                // Webhook
                rect0.width = labelWidth;
                GUI.Label(rect0, "Webhook");
                rect0.x += rect0.width;

                rect0.width = containerRect.width - (textWidth) - labelWidth * 3 - 20;
                string dw = element.WebhookURL;
                if (showWebhook)
                {
                    string t = GUI.TextField(rect0, dw);
                    if (t != dw)
                    {
                        element.WebhookURL = t;
                        dirty = true;
                    }
                }
                else
                {
                    dw = new string('*', dw.Length);
                    GUI.Label(rect0, dw);
                }
                rect0.x += rect0.width;

                // Padding
                rect0.width = 10;
                GUI.Label(rect0, "");
                rect0.x += rect0.width;

                // Show Toggle
                rect0.width = 70;
                showWebhook = GUI.Toggle(rect0, showWebhook, "Show");
            }
        }

        protected override GoogleConfig.GoogleChatSpace CreateItem(int index)
        {
            return new GoogleConfig.GoogleChatSpace(index, "MyChatSpace");
        }

        protected override int CompareTo(GoogleConfig.GoogleChatSpace a, GoogleConfig.GoogleChatSpace b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
