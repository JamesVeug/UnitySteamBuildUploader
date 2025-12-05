using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfSlackChannels : InternalReorderableList<SlackConfig.SlackChannel>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                SlackConfig.SlackChannel element = list[index];

                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.Name);
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
                
                // Channel ID
                rect1.width = 200;
                string newChannelID = GUI.TextField(rect1, element.ChannelID);
                if (newChannelID != element.ChannelID)
                {
                    element.ChannelID = newChannelID;
                    dirty = true;
                }
                rect1.x += rect1.width;
            }
        }

        protected override SlackConfig.SlackChannel CreateItem(int index)
        {
            return new SlackConfig.SlackChannel(index, "BotTestChannel", "");
        }
        
        protected override int CompareTo(SlackConfig.SlackChannel a, SlackConfig.SlackChannel b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}