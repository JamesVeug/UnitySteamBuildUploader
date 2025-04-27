using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class ABrowsePathSource
    {
        private string ButtonText => GetType().GetCustomAttribute<BuildSourceAttribute>()?.ButtonText ?? GetType().Name;
        
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        protected internal abstract string SelectFile();

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
                GUILayout.Label("Path Type:", GUILayout.Width(120));

                var newPathType = (PathType)EditorGUILayout.EnumPopup(m_pathType);
                if (newPathType != m_pathType)
                {
                    m_pathType = newPathType;
                    isDirty = true;
                }
            }

            bool exists = PathExists();
            GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Path:", GUILayout.Width(120));
                
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
                    EditorUtility.RevealInFinder(GetFullPath());
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(GetFullPath(), style);
                }
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            Setup();
            
            bool exists = PathExists();
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            style.alignment = TextAnchor.MiddleLeft;
            
            string displayedPath = Utils.TruncateText(GetFullPath(), maxWidth, ButtonText);
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = SelectFile();
                if (m_pathType != PathType.Absolute)
                {
                    string subPath = GetSubPath();
                    if (!newPath.StartsWith(subPath))
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "The selected path just start in:\n" + subPath + "\n\nOr change the PathType of the source." , "OK");
                        return;
                    }
                }
                
                isDirty |= SetNewPath(newPath);
            }
        }
    }
}