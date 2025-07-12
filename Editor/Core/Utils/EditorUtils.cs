using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class EditorUtils
    {
        public static readonly string FormatStringTextFieldTooltip = "Show the text as it will appear after been formatted.\n\n" +
                                                                     "See docs to see available string formats such as {version} to get " +
                                                                     Application.version;
        
        public static bool FormatStringTextField(ref string text, ref bool pressed, GUILayoutOption textFieldOption)
        {
            return FormatStringTextField(ref text, ref pressed, null, textFieldOption);
        }
        
        public static bool FormatStringTextField(ref string text, ref bool pressed, GUIStyle style = null, GUILayoutOption textFieldOption = null)
        {
            if (style == null)
            {
                style = EditorStyles.textField;
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent content = new GUIContent("F", FormatStringTextFieldTooltip);
                
                var newPressed = GUILayout.Toggle(pressed, content, "ToolbarButton", GUILayout.Width(20), GUILayout.Height(20));
                if (newPressed != pressed)
                {
                    pressed = newPressed;
                    GUI.FocusControl(null); // Deselect the text field so we can see the formatted text
                }

                using (new EditorGUI.DisabledScope(pressed))
                {
                    string displayText = pressed ? StringFormatter.FormatString(text) : text;
                    string newText = "";
                    
                    if (textFieldOption == null)
                    {
                        newText = EditorGUILayout.TextField(displayText, style);
                    }
                    else
                    {
                        newText = EditorGUILayout.TextField(displayText, style, textFieldOption);
                    }

                    if (!pressed && newText != text)
                    {
                        text = newText;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
