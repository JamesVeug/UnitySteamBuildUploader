using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public static class PasswordField
    {
        private static Dictionary<string, bool> m_passwordFieldToggles = new Dictionary<string, bool>();
        public static string Draw(string label, int labelLength, string password, char mask = '*')
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(labelLength));
                
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