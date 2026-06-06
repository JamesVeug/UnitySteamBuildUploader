using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDiscordAppsProjectSettings : InternalReorderableList<DiscordConfig.DiscordApp>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DiscordConfig.DiscordApp element = list[index];

                float labelWidth = 50;
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Name", "Display name for this Discord bot/app. UI only — not sent to Discord."));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. Release Bot");
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
                
                // Is Bot Toggle
                rect1.width = 75;
                element.IsBot = GUI.Toggle(rect1, element.IsBot, new GUIContent("Is Bot", "Tick if this app authenticates as a bot account rather than a webhook/user."));
                rect1.x += rect1.width;
            }
        }

        protected override DiscordConfig.DiscordApp CreateItem(int index)
        {
            return new DiscordConfig.DiscordApp(index, "MyBot");
        }
        
        protected override int CompareTo(DiscordConfig.DiscordApp a, DiscordConfig.DiscordApp b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}