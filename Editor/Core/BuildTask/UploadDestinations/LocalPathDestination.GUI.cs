using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class LocalPathDestination
    {
        private string ButtonText => "Choose Local Path...";
        
        private GUIStyle m_pathButtonExistsStyle;
        private GUIStyle m_pathButtonDoesNotExistStyle;
        private GUIStyle m_pathInputFieldExistsStyle;
        private GUIStyle m_pathInputFieldDoesNotExistStyle;

        private void Setup()
        {
            m_pathButtonExistsStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle.normal.textColor = Color.red;
            
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.red;
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
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
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));

                bool newZip = EditorGUILayout.Toggle(m_zipContent, GUILayout.Width(20));
                if (m_zipContent != newZip)
                {
                    m_zipContent = newZip;
                    isDirty = true;
                }

                using (new EditorGUI.DisabledScope(!m_zipContent))
                {
                    GUILayout.Label("Name (No extension):", GUILayout.Width(125));
                    string newPath = GUILayout.TextField(m_fileName);
                    if (m_fileName != newPath)
                    {
                        m_fileName = newPath;
                        isDirty = true;
                    }
                }
            }

            GUILayout.Label("Full Path: " + FullPath());
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
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

        private string GetButtonText(float maxWidth)
        {
            string fullPath = FullPath();
            string displayedPath = fullPath;
            if (!string.IsNullOrEmpty(displayedPath))
            {
                float characterWidth = 8f;
                int characters = displayedPath.Length;
                float expectedWidth = characterWidth * characters;
                if (expectedWidth >= maxWidth)
                {
                    int charactersToRemove = (int)((expectedWidth - maxWidth) / characterWidth);
                    if (charactersToRemove < displayedPath.Length)
                    {
                        displayedPath = displayedPath.Substring(charactersToRemove);
                    }
                    else
                    {
                        displayedPath = "";
                    }
                }
                
                if(displayedPath.Length < fullPath.Length)
                {
                    displayedPath = "..." + displayedPath;
                }
            }
            else
            {
                displayedPath = ButtonText;
            }

            return displayedPath;
        }

        protected virtual string SelectFile()
        {
            return EditorUtility.OpenFolderPanel("Select Folder to upload", m_localPath, "");
        }
    }
}