using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class CustomPathTextField
    {
        private static GUIStyle m_pathInputFieldExistsStyle;
        private static GUIStyle m_pathInputFieldDoesNotExistStyle;
        private static GUIStyle m_pathLabelExistsStyle;
        private static GUIStyle m_pathLabelDoesNotExistStyle;
        
        static CustomPathTextField()
        {
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.yellow;
            
            m_pathLabelExistsStyle = new GUIStyle(GUI.skin.label);
            m_pathLabelDoesNotExistStyle = new GUIStyle(GUI.skin.label);
            m_pathLabelDoesNotExistStyle.normal.textColor = Color.yellow;
        }

        public static bool OnGUI(ref string unformattedPath, ref bool showFormatted, StringFormatter.Context ctx)
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
                    string newPath = EditorUtility.OpenFolderPanel("Select Folder to upload", unformattedPath, "");
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