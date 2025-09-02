using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDiscordChannels : InternalReorderableList<DiscordConfig.DiscordChannel>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DiscordConfig.DiscordChannel element = list[index];

                float width = Mathf.Min(100, rect.width / 2);
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
                string c = GUI.TextField(rect1, element.ChannelID.ToString());
                if (long.TryParse(c, out long newID) && newID != element.ChannelID)
                {
                    element.ChannelID = newID;
                    dirty = true;
                }
                rect1.x += rect1.width;
            }
        }

        protected override DiscordConfig.DiscordChannel CreateItem(int index)
        {
            return new DiscordConfig.DiscordChannel(index, "BotTestChannel", -1);
        }
        
        protected override int CompareTo(DiscordConfig.DiscordChannel a, DiscordConfig.DiscordChannel b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}