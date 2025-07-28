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
        private GUIStyle m_pathLabelExistsStyle;
        private GUIStyle m_pathLabelDoesNotExistStyle;
        
        private bool m_showFormattedLocalPath = false;
        private bool m_showFormattedZippedFilesName = false;

        private void Setup()
        {
            m_pathButtonExistsStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle = new GUIStyle(GUI.skin.button);
            m_pathButtonDoesNotExistStyle.normal.textColor = Color.yellow;
            
            m_pathInputFieldExistsStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle = new GUIStyle(GUI.skin.textField);
            m_pathInputFieldDoesNotExistStyle.normal.textColor = Color.yellow;
            
            m_pathLabelExistsStyle = new GUIStyle(GUI.skin.label);
            m_pathLabelDoesNotExistStyle = new GUIStyle(GUI.skin.label);
            m_pathLabelDoesNotExistStyle.normal.textColor = Color.yellow;
        }

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            Setup();

            bool exists = PathExists(ctx);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Directory:", "Where the contents of the sources should be copied to.");
                GUILayout.Label(label, GUILayout.Width(120));

                GUIStyle style = exists ? m_pathInputFieldExistsStyle : m_pathInputFieldDoesNotExistStyle;
                if (EditorUtils.FormatStringTextField(ref m_localPath, ref m_showFormattedLocalPath, ctx, style))
                {
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
                    GUIContent label = new GUIContent("Name (No extension):", "Name of the zipped file that will be created." +
                                                                              "\nSee docs for format options such as {buildNumber} and {date}.");
                    GUILayout.Label(label, GUILayout.Width(125));
                    
                    if (EditorUtils.FormatStringTextField(ref m_zippedFilesName, ref m_showFormattedZippedFilesName, ctx))
                    {
                        isDirty = true;
                    }
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Duplicate Files: ", GUILayout.Width(120));
                var newHandler = (Utils.FileExistHandling)EditorGUILayout.EnumPopup(m_duplicateFileHandling);
                if (m_duplicateFileHandling != newHandler)
                {
                    m_duplicateFileHandling = newHandler;
                    isDirty = true;
                }
            }

            GUIStyle fullPathStyle = exists ? m_pathLabelExistsStyle : m_pathLabelDoesNotExistStyle;
            fullPathStyle.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label("Full Path: " + FullPath(ctx), fullPathStyle);
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            Setup();

            bool exists = PathExists(ctx);
            GUIStyle style = exists ? m_pathButtonExistsStyle : m_pathButtonDoesNotExistStyle;
            style.alignment = TextAnchor.MiddleLeft;

            string displayedPath = GetButtonText(maxWidth, ctx);
            if (GUILayout.Button(displayedPath, style))
            {
                string newPath = SelectFile();
                isDirty |= SetNewPath(newPath);
            }
        }

        private string GetButtonText(float maxWidth, StringFormatter.Context ctx)
        {
            string fullPath = FullPath(ctx);
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