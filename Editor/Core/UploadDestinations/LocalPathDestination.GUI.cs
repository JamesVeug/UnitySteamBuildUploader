using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class LocalPathDestination
    {
        internal override void OnGUIExpanded(ref bool isDirty)
        {
            Setup();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Directory:", GUILayout.Width(120));

                bool exists = !string.IsNullOrEmpty(m_localPath) && Directory.Exists(m_localPath);
                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                string newPath = GUILayout.TextField(m_localPath, style);
                if (m_localPath != newPath)
                {
                    m_localPath = newPath;
                    isDirty = true;
                }

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string path = SelectFile();
                    isDirty |= SetNewPath(path);
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_localPath);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Name (No extension):", GUILayout.Width(120));
                string newPath = GUILayout.TextField(m_fileName);
                if (m_fileName != newPath)
                {
                    m_fileName = newPath;
                    isDirty = true;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));

                bool newZip = EditorGUILayout.Toggle(m_zipContent);
                if (m_zipContent != newZip)
                {
                    m_zipContent = newZip;
                    isDirty = true;
                }
            }

            GUILayout.Label("Full Path: " + FullPath());
        }

        internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            Setup();

            bool exists = PathExists();
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            style.alignment = TextAnchor.MiddleLeft;

            string displayedPath = GetButtonText(maxWidth);
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = SelectFile();
                isDirty |= SetNewPath(newPath);
            }
        }
    }
}