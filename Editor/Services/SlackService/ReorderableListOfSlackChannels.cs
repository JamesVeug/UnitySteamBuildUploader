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

                float labelWidth = 50;
                float width = Mathf.Min(200, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this channel. UI only — not sent to Slack."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. #release-builds");
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
                GUI.Label(rect1, new GUIContent("Channel ID", "Slack channel ID. Open the channel -> View channel details -> the ID is at the bottom."));
                rect1.x += rect1.width;
                rect1.width = 200;
                string newChannelID = EditorUtils.PlaceholderTextField(rect1, element.ChannelID, "e.g. C0123456789");
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