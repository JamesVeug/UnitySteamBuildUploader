using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class CustomFolderPathTextField
    {
        private static GUIStyle m_pathInputFieldExistsStyle;
        private static GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        static CustomFolderPathTextField()
        {
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.yellow;
        }

        public static bool OnGUI(string title, ref string unformattedPath, ref bool showFormatted, StringFormatter.Context ctx)
        {
            bool exists = Utils.PathExists(StringFormatter.FormatString(unformattedPath, ctx));
            bool isDirty = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                if (EditorUtils.FormatStringTextField(ref unformattedPath, ref showFormatted, ctx, style))
                {
                    isDirty = true;
                }

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string newPath = EditorUtility.OpenFolderPanel(title, unformattedPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        unformattedPath = newPath; // This is an unformatted path
                        isDirty = true;
                    }
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(StringFormatter.FormatString(unformattedPath, ctx));
                }
            }
            
            return isDirty;
        }
    }
}