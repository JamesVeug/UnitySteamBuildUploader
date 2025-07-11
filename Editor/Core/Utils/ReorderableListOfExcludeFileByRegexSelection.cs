using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfExcludeFileByRegexSelection : InternalReorderableList<AExcludePathsByRegex_BuildModifier.Selection>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            AExcludePathsByRegex_BuildModifier.Selection element = list[index];
            
            Rect rect0 = new Rect(rect.x, rect.y, Mathf.Min(20, rect.width / 4), rect.height);
            bool e = GUI.Toggle(rect0, element.Enabled, "");
            if (e != element.Enabled)
            {
                element.Enabled = e;
                dirty = true;
            }
            
            string regex = element.Regex;
            bool valid = true;
            try
            {
                System.Text.RegularExpressions.Regex.IsMatch("", regex);
            }
            catch
            {
                valid = false;
            }
            
            
            
            Rect rect1 = new Rect(rect0.x + rect0.width, rect.y, Mathf.Min(120, rect.width / 4), rect.height);
            GUIStyle style = valid ? GUI.skin.textField : new GUIStyle(GUI.skin.textField) { normal = { textColor = Color.red } };
            string n = GUI.TextField(rect1, regex, style);
            if (n != regex)
            {
                element.Regex = n;
                dirty = true;
            }
            
            Rect rect2 = new Rect(rect1.x + rect1.width + 20, rect.y, Mathf.Min(150, rect.width / 4), rect.height);
            bool b2 = GUI.Toggle(rect2, element.SearchAllDirectories, "Search all Directories");
            if (b2 != element.SearchAllDirectories)
            {
                element.SearchAllDirectories = b2;
                dirty = true;
            }
            
            Rect rect3 = new Rect(rect2.x + rect2.width + 10, rect.y, Mathf.Min(200, rect.width / 4), rect.height);
            bool b = GUI.Toggle(rect3, element.Recursive, "Delete Recursively");
            if (b != element.Recursive)
            {
                element.Recursive = b;
                dirty = true;
            }
        }

        protected override AExcludePathsByRegex_BuildModifier.Selection CreateItem(int index)
        {
            return new AExcludePathsByRegex_BuildModifier.Selection();
        }
    }
}