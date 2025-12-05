using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfSlackMessageAttachments : InternalReorderableList<Slack.Attachment>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Slack.Attachment element = list[index];

                // Title
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                
                // Description
                rect1.width = 200;
                string d = GUI.TextArea(rect1, element.text);
                if (d != element.text)
                {
                    element.text = d;
                    dirty = true;
                }
                
                
                
                rect1.x += rect1.width;
                rect1.width = 70;
                
                Color newColor = Color.white;
                try
                {
                    string tC = element.color;
                    if (tC.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        tC = "#" + tC.Substring(2);
                    }
                    else if (!tC.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        tC = "#" + tC;
                    }
                    
                    if (!ColorUtility.TryParseHtmlString(tC, out newColor))
                    {
                        newColor = Color.white;
                    }
                }
                catch
                {
                    // ignored
                }

                rect1.x += rect1.width;
                rect1.width = 20;
                int pictureSize = (int)rect1.width * 10;
                Texture2D t2 = new Texture2D(pictureSize, pictureSize);
                for (int i = 0; i < pictureSize; i++)
                {
                    for (int j = 0; j < pictureSize; j++)
                    {
                        t2.SetPixel(i, j, newColor);
                    }
                }
                t2.Apply();
                if (GUI.Button(rect1, t2, GUI.skin.button))
                {
                    Application.OpenURL("https://www.w3schools.com/colors/colors_hexadecimal.asp");
                }
            }
        }

        protected override Slack.Attachment CreateItem(int index)
        {
            return new Slack.Attachment()
            {
                text = ":white_check_mark: #123456 *Successfully* uploaded!",
                fallback = "",
                color = "#009000",
                attachment_type = "",
                callback_id = "",
                actions = new List<Slack.Attachment.Action>()
            };
        }
        
        protected override int CompareTo(Slack.Attachment a, Slack.Attachment b)
        {
            return String.Compare(a.text, b.text, StringComparison.Ordinal);
        }
    }
}