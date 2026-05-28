using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Preferences list: shows the app Name and a masked client-secret field.
    /// The secret is stored in EditorPrefs and is never saved to the JSON config.
    /// </summary>
    public class ReorderableListOfXboxAppsPreferences : InternalReorderableList<XboxConfig.XboxApp>
    {
        private bool[] m_showSecret;

        protected override void DrawItem(Rect containerRect, int index, bool isActive, bool isFocused)
        {
            if (m_showSecret == null || m_showSecret.Length != list.Count)
                m_showSecret = new bool[list.Count];

            XboxConfig.XboxApp element = list[index];

            float toggleW = 22f;
            float labelW  = 80f;
            float nameW   = 140f;
            float secretW = containerRect.width - labelW * 2f - nameW - toggleW;

            Rect r = new Rect(containerRect.x, containerRect.y, labelW, containerRect.height);

            GUI.Label(r, "Name");
            r.x += r.width;
            r.width = nameW;
            using (new EditorGUI.DisabledScope(true))
                GUI.TextField(r, element.Name);
            r.x += r.width;

            r.width = labelW;
            GUI.Label(r, "Client Secret");
            r.x += r.width;

            r.width = toggleW;
            m_showSecret[index] = GUI.Toggle(r, m_showSecret[index], m_showSecret[index] ? "H" : "S");
            r.x += r.width;

            r.width = secretW;
            string current = element.ClientSecret;
            string newVal = m_showSecret[index]
                ? GUI.TextField(r, current)
                : GUI.PasswordField(r, current, '*');
            if (newVal != current)
                element.ClientSecret = newVal;
        }

        protected override XboxConfig.XboxApp CreateItem(int index)
        {
            return new XboxConfig.XboxApp(index, "My Xbox App");
        }

        protected override int CompareTo(XboxConfig.XboxApp a, XboxConfig.XboxApp b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}
