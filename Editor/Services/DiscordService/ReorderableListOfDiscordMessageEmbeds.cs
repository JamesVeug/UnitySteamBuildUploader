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
                rect1.width = 100;
                
                Color t = GUI.color;
                try
                {
                    string tC = element.color;
                    if (!tC.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        tC = "#" + tC;
                    }
                    
                    if (ColorUtility.TryParseHtmlString(tC, out Color color))
                    {
                        GUI.color = color;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                }
                catch
                {
                    // ignored
                }

                string c = GUI.TextArea(rect1, element.color);
                if (c != element.color)
                {
                    element.color = c;
                    dirty = true;
                }
                
                GUI.color = t;
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