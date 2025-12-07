using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class EditorUtils
    {
        public static string GetFormatStringTextFieldTooltip(Context ctx)
        {
            StringBuilder tooltipBuilder = new StringBuilder();
            tooltipBuilder.AppendLine("Show the text as it will appear with formats:");

            const int maximum = 30;
            int ignored = 0;
            List<string> commandKeys = new List<string>(maximum);
            List<Command> commands = new List<Command>(maximum);
            foreach (Command command in ctx.LocalCommands.Where(a=>a.Key.Length > 2))
            {
                commands.Add(command);
                commandKeys.Add(command.Key);
            }
            
            foreach (Command command in Context.FormatToCommand.Values.OrderBy(a=>a.Key))
            {
                if (command.Key.Length <= 2 || commandKeys.Contains(command.Key))
                {
                    continue;
                }
                
                if (commands.Count >= maximum)
                {
                    ignored++;
                    continue;
                }
                
                commands.Add(command);
                commandKeys.Add(command.Key);
            }
            commands.Sort();
            
            
            foreach (Command command in commands)
            {
                tooltipBuilder.Append(command.Key);
                tooltipBuilder.Append(" - ");
                tooltipBuilder.AppendLine(ctx.FormatString(command.Key));
            }

            if (commands.Count >= maximum)
            {
                tooltipBuilder.AppendLine($"+{ignored} more...");
                tooltipBuilder.AppendLine();
                tooltipBuilder.AppendLine("For all format see the Wiki:\nWindow->Build Uploader->Welcome->Documentation");
            }
            
            return tooltipBuilder.ToString();
        }

        public static bool FormatStringTextField(ref string text, ref bool pressed, Context ctx, GUILayoutOption textFieldOption)
        {
            return FormatStringTextField(ref text, ref pressed, ctx, null, textFieldOption);
        }
        
        public static bool FormatStringTextField(ref string text, ref bool pressed, Context ctx, GUIStyle style = null, GUILayoutOption textFieldOption = null)
        {
            return FormatStringText(ref text, ref pressed, style, textFieldOption, true, ctx);
        }

        public static bool FormatStringTextArea(ref string text, ref bool pressed, GUILayoutOption textFieldOption)
        {
            return FormatStringTextField(ref text, ref pressed, null, textFieldOption);
        }
        
        public static bool FormatStringTextArea(ref string text, ref bool pressed, Context ctx, GUIStyle style = null, GUILayoutOption textFieldOption = null)
        {
            return FormatStringText(ref text, ref pressed, style, textFieldOption, false, ctx);
        }
        
        private static bool FormatStringText(ref string text, ref bool pressed, GUIStyle style, GUILayoutOption textFieldOption, bool textField, Context ctx)
        {
            if (style == null)
            {
                style = EditorStyles.textField;
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent content = new GUIContent("F", GetFormatStringTextFieldTooltip(ctx));

                GUIStyle guiStyle = "ToolbarButton";
                var newPressed = GUILayout.Toggle(pressed, content, guiStyle, GUILayout.Width(20), GUILayout.Height(20));
                if (newPressed != pressed)
                {
                    pressed = newPressed;
                    GUI.FocusControl(null); // Deselect the text field so we can see the formatted text
                }

                using (new EditorGUI.DisabledScope(pressed))
                {
                    string displayText = pressed ? ctx.FormatString(text) : text;
                    string newText = "";
                    
                    if (textFieldOption == null)
                    {
                        if(textField)
                            newText = EditorGUILayout.TextField(displayText, style);
                        else
                            newText = EditorGUILayout.TextArea(displayText, style);
                    }
                    else
                    {
                        if(textField)
                            newText = EditorGUILayout.TextField(displayText, style, textFieldOption);
                        else
                            newText = EditorGUILayout.TextArea(displayText, style, textFieldOption);
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

        public static bool DrawUploadProfileDropdown(ref UploadProfileMeta selectedProfile, List<UploadProfileMeta> profiles, Context ctx)
        {
            List<string> profileNames = new List<string>();
            profileNames.Add("-- Select Upload Profile --");
                    
            profileNames.AddRange(profiles.Select(p => ctx.FormatString(p.ProfileName)));
            for (int i = 1; i < profileNames.Count; i++)
            {
                profileNames[i] = $"{i}. {profileNames[i]}";
            }

            int selectedIndex = 0;
            if (selectedProfile != null)
            {
                string guid = selectedProfile.GUID;
                selectedIndex = profiles.FindIndex(a => a.GUID == guid);
                if (selectedIndex != -1)
                {
                    selectedIndex++;
                }
            }

            var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, profileNames.ToArray(), GUILayout.Width(150));
            if (newSelectedIndex == selectedIndex)
            {
                return false;
            }

            if (newSelectedIndex <= 0)
            {
                selectedProfile = null;
            }
            else
            {
                selectedProfile = profiles[newSelectedIndex - 1];
            }

            return true;
        }

        public static void DrawPopup<T>(List<T> selected, List<T> allOptions, string emptySelection, Action<List<T>> callback, params GUILayoutOption[] options) where T : DropdownElement
        {
            // TODO: Replace this with the actual popup with more lists/array shit?
            string buttonText = selected.Count == 0 ? emptySelection : string.Join(",", selected.Select(a=>a.DisplayName));
            GUIStyle style = new GUIStyle(EditorStyles.popup);
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(buttonText), style, options);
            if (GUI.Button(buttonRect, buttonText, style)) 
            {
                List<T> m_channels = new List<T>(selected);
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear"), selected.Count == 0, () =>
                {
                    m_channels.Clear();
                    callback(m_channels);
                });
                
                foreach (T channel in allOptions.OrderBy(a=>a.DisplayName))
                {
                    bool isSelected = selected.Contains(channel);
                    menu.AddItem(new GUIContent(channel.DisplayName), isSelected, () =>
                    {
                        if (isSelected)
                        {
                            m_channels.Remove(channel);
                        }
                        else
                        {
                            m_channels.Add(channel);
                            m_channels.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
                        }

                        callback(m_channels);
                    });
                }
                
                Rect rect = buttonRect;
                // rect.y += rect.height;
                menu.DropDown(rect);
            }
        }
    }
}
