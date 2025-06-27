using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfBranches : InternalReorderableList<SteamBranch>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                SteamBranch element = list[index];

                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                string n = GUI.TextField(rect1, element.name);
                if (n != element.name)
                {
                    element.name = n;
                    dirty = true;
                }

                if (n == "default")
                {
                    rect1.x += width;
                    
                    // Warning - uploading to default branch is not allowed!
                    rect1.width = 15;
                    
                    Color color = GUI.color;
                    GUI.color = new Color(1f,0.5f,0f);
                    GUI.Label(rect1, "!!!");
                    GUI.color = color;
                    
                    rect1.x += 15;
                    rect1.width = rect.width - width - 15;
                    GUI.Label(rect1, "Uploading to the 'default' branch is not allowed by SteamSDK. Upload to none or an empty branch name then use the dashboard to assign to default.");
                }
            }
        }

        protected override SteamBranch CreateItem(int index)
        {
            return new SteamBranch(index, "");
        }
    }
}