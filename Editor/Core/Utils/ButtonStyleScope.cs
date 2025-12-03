using System;
using UnityEngine;

namespace Wireframe
{
    internal struct ButtonStyleScope : IDisposable
    {
        private static readonly GUIStyle standardStyle;
        
        // Dark
        private static readonly GUIStyle warningStyle;
        private static readonly Color warningColor;
        private static readonly GUIStyle errorStyle;
        private static readonly Color errorColor;
        
        // Light
        private static readonly GUIStyle warningStyleLight;
        private static readonly Color warningColorLight;
        private static readonly GUIStyle errorStyleLight;
        private static readonly Color errorColorLight;

        static ButtonStyleScope()
        {
            standardStyle = new GUIStyle(GUI.skin.button);
            standardStyle.alignment = TextAnchor.MiddleLeft;
            
            // Dark
            warningStyle = new GUIStyle(GUI.skin.button);
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.normal.textColor =  new Color(0.86f, 0.86f, 0f);
            warningColor = new Color(0.8647059f,0.84901965f,0.6843138f,1);
            
            errorStyle = new GUIStyle(GUI.skin.button);
            errorStyle.alignment = TextAnchor.MiddleLeft;
            errorStyle.normal.textColor = Color.white; // White because red text is too hard to read for me
            errorColor = new Color(0.7f, 0.4f, 0.4f, 1);
            
            // Light
            warningStyleLight = new GUIStyle(GUI.skin.button);
            warningStyleLight.alignment = TextAnchor.MiddleLeft;
            warningStyleLight.normal.textColor = Color.black;
            warningColorLight = new Color(0.8647059f,0.84901965f,0.6843138f,1);
            
            errorStyleLight = new GUIStyle(GUI.skin.button);
            errorStyleLight.alignment = TextAnchor.MiddleLeft;
            errorStyleLight.normal.textColor = Color.black;
            errorColorLight = new Color(0.9647059f, 0.5803922f, 0.5803922f, 1);
        }
        
        public readonly GUIStyle style;
        
        private bool m_Disposed;
        private Color m_PreviousColor;

        public ButtonStyleScope(bool warning, bool error)
        {
            this.m_Disposed = false;
            this.m_PreviousColor = GUI.color;

            if (!GUI.enabled || (!warning && !error))
            {
                GUI.color = Color.white;
                style = standardStyle;
            }
            else if (Utils.IsDarkMode)
            {
                GUI.color = error ? errorColor : warningColor;
                style = error ? errorStyle : warningStyle;
            }
            else
            {
                GUI.color = error ? errorColorLight : warningColorLight;
                style = error ? errorStyleLight : warningStyleLight;
            }
        }

        public void Dispose()
        {
            if (this.m_Disposed)
                return;
            this.m_Disposed = true;
            GUI.color = this.m_PreviousColor;
        }
    }
}