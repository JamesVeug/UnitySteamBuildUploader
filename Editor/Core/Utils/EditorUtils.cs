using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class EditorUtils
    {
        public static string GetFormatStringTextFieldTooltip()
        {
            StringBuilder tooltipBuilder = new StringBuilder();
            tooltipBuilder.AppendLine("Show the text as it will appear with formats:");

            int maximum = 20;
            foreach (StringFormatter.Command command in StringFormatter.Commands.OrderBy(a=>a.Key))
            {
                if (maximum-- <= 0)
                {
                    tooltipBuilder.AppendLine("\n...See docs to see available string formats.");
                    break;
                }
                
                tooltipBuilder.Append(command.Key);
                tooltipBuilder.Append(" - ");
                tooltipBuilder.AppendLine(command.Formatter());
            }
            
            return tooltipBuilder.ToString();
        }

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
                GUIContent content = new GUIContent("F", GetFormatStringTextFieldTooltip());
                
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
