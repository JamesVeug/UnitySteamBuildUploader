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

                float labelWidth = 50;
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this channel. UI only — not sent to Discord."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. release-announcements");
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
                rect1.width = 70;
                GUI.Label(rect1, new GUIContent("Channel ID", "Discord channel ID. Can be found in the browsers url"));
                rect1.x += rect1.width;
                rect1.width = 200;
                string c = EditorUtils.PlaceholderTextField(rect1, element.ChannelID.ToString(), "e.g. 123456789012345678");
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