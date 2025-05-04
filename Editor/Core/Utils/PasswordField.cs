using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public static class PasswordField
    {
        private static Dictionary<string, bool> m_passwordFieldToggles = new Dictionary<string, bool>();
        public static string Draw(string label, string tooltip, int labelLength, string password, char mask = '*', Action onHelpPressed = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                int realLabelLength = labelLength;
                if (onHelpPressed != null)
                {
                    realLabelLength -= 20;
                }
                
                GUILayout.Label(new GUIContent(label, tooltip), GUILayout.Width(realLabelLength));
                
                if (onHelpPressed != null)
                {
                    if (GUILayout.Button("?", GUILayout.Width(20)))
                    {
                        onHelpPressed();
                    }
                }
                
                if (password == null)
                {
                    password = "";
                }

                if (!m_passwordFieldToggles.TryGetValue(label, out bool showPassword))
                {
                    m_passwordFieldToggles.Add(label, false);
                }

                string newPassword = null;
                if (showPassword)
                {
                    newPassword = GUILayout.TextField(password);
                    
                }
                else
                {
                    newPassword = GUILayout.PasswordField(password, mask);
                }
                
                if (GUILayout.Button(showPassword ? "Hide" : "Show", GUILayout.Width(50)))
                {
                    m_passwordFieldToggles[label] = !showPassword;
                }
                
                return newPassword;
            }
            
        }
    }
}