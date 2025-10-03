﻿using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class LocalPathDestination
    {
        private string ButtonText => "Choose Local Path...";
        
        private bool m_showFormattedLocalPath = false;
        private bool m_showFormattedZippedFilesName = false;

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            isDirty |= CustomFolderPathTextField.OnGUI(ref m_localPath, ref m_showFormattedLocalPath, ctx);

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
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            string displayedPath = FullPath(ctx);
            if (CustomPathButton.OnGUI(ref displayedPath, ButtonText, maxWidth))
            {
                m_localPath = displayedPath; // If this is changed we are given a non-formatted version
                isDirty = true;
            }
        }
    }
}