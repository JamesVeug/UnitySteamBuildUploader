using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class ABrowsePathSource
    {
        internal string ButtonText => GetType().GetCustomAttribute<BuildSourceAttribute>()?.ButtonText ?? GetType().Name;

        internal abstract string SelectFile();

        private void Setup()
        {
            m_pathButtonExistsStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle.normal.textColor = Color.red;
            
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.red;
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            Setup();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Path:", GUILayout.Width(120));
                
                bool exists = !string.IsNullOrEmpty(m_enteredFilePath) && (File.Exists(m_enteredFilePath) || Directory.Exists(m_enteredFilePath));
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                string newPath = GUILayout.TextField(m_enteredFilePath, style, GUILayout.MaxWidth(200));
                if (m_enteredFilePath != newPath)
                {
                    m_enteredFilePath = newPath;
                    isDirty = true;
                }

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string path = SelectFile();
                    isDirty |= SetNewPath(path);
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_enteredFilePath);
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            Setup();
            
            bool exists = PathExists();
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            style.alignment = TextAnchor.MiddleLeft;
            
            string displayedPath = Utils.TruncateText(m_enteredFilePath, maxWidth, ButtonText);
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = SelectFile();
                isDirty |= SetNewPath(newPath);
            }
        }
    }
}