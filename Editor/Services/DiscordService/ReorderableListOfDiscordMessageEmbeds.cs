using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDiscordMessageEmbeds : InternalReorderableList<DiscordMessageChannelAction.Embed>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DiscordMessageChannelAction.Embed element = list[index];

                // Title
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.title);
                if (n != element.title)
                {
                    element.title = n;
                    dirty = true;
                }
                
                // Padding
                rect1.x += rect1.width;
                rect1.width = 10;
                GUI.Label(rect1, "");
                rect1.x += rect1.width;
                
                // Description
                rect1.width = 200;
                string d = GUI.TextArea(rect1, element.description);
                if (d != element.description)
                {
                    element.description = d;
                    dirty = true;
                }
                
                
                
                rect1.x += rect1.width;
                rect1.width = 70;

                string c = GUI.TextArea(rect1, element.color);
                if (c != element.color)
                {
                    element.color = c;
                    dirty = true;
                }
                
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

        protected override DiscordMessageChannelAction.Embed CreateItem(int index)
        {
            return new DiscordMessageChannelAction.Embed()
            {
                title = "v{version}",
                description = "Bug Fixes",
                color = "#009000"
            };
        }
    }
}