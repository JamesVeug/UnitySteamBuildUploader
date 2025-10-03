using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class CustomFilePathTextField
    {
        private static GUIStyle m_pathInputFieldExistsStyle;
        private static GUIStyle m_pathInputFieldDoesNotExistStyle;
        
        static CustomFilePathTextField()
        {
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.yellow;
        }

        public static bool OnGUI(ref string unformattedPath, ref bool showFormatted, StringFormatter.Context ctx, string fileTypes="*")
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
                    string newPath = EditorUtility.OpenFilePanel("Select Folder to upload", unformattedPath, fileTypes);
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

            if (isDirty)
            {
                // Make sure there are no invalid characters copied over like double quotes at the start or end
                string directory = "";
                int lastSlash = unformattedPath.LastIndexOfAny(new char[] {'/', '\\'});
                if (lastSlash > 0)
                {
                    directory = unformattedPath.Substring(0, lastSlash);
                    foreach (char invalidChar in Path.GetInvalidPathChars())
                    {
                        directory = directory.Replace(invalidChar.ToString(), "");
                    }
                }
                
                string fileName = lastSlash >= 0 ? unformattedPath.Substring(lastSlash + 1) : unformattedPath;
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(invalidChar.ToString(), "");
                }
                
                unformattedPath = Path.Combine(directory, fileName);
            }
            
            return isDirty;
        }
    }
}