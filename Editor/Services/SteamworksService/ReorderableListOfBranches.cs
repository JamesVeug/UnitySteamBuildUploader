using System;
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

                float labelWidth = 55;
                float width = Mathf.Min(100, rect.width / 2);
                Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
                GUI.Label(rect1, new GUIContent("Branch", "Steam branch (beta) to set this build live on. Leave empty to upload without setting live.\nNOTE: 'default' is not allowed by the SteamSDK. That requires manually switching on the dashboard"));
                rect1.x += rect1.width;
                rect1.width = width;
                string n = EditorUtils.PlaceholderTextField(rect1, element.name, "e.g. beta (leave empty to not set live)");
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
                    rect1.width = rect.width - width - 15 - labelWidth;
                    GUI.Label(rect1, "Uploading to the 'default' branch is not allowed by SteamSDK. Upload to none or an empty branch name then use the dashboard to assign to default.");
                }
            }
        }

        protected override SteamBranch CreateItem(int index)
        {
            return new SteamBranch(index, "");
        }

        protected override int CompareTo(SteamBranch a, SteamBranch b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}