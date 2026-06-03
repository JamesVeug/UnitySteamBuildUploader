using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class DropboxDestination
    {
        private bool m_showFormattedFileName = Preferences.DefaultShowFormattedTextToggle;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= DropboxUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(120));
            isDirty |= DropboxUIUtils.FolderPopup.DrawPopup(ref m_folder, m_context, GUILayout.Width(120));

            float width = maxWidth - (120 * 2);
            using (new EditorGUI.DisabledScope(true))
            {
                bool alwaysFormatted = true;
                EditorUtils.FormatStringTextArea(ref m_fileNameFormat, ref alwaysFormatted, m_context, null, GUILayout.Width(width));
            }
        }

        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                Application.OpenURL("https://www.dropbox.com/developers/documentation/http/documentation#files-upload");
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= DropboxUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(200));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Folder:", GUILayout.Width(120));
                isDirty |= DropboxUIUtils.FolderPopup.DrawPopup(ref m_folder, m_context, GUILayout.Width(200));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("File Name:", "The name the uploaded file will appear as on Dropbox.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_fileNameFormat, ref m_showFormattedFileName, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_zipContents);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Create Shared Link:", "Create a public shared link exposed via {dropboxShareLink}.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createShareLink);
            }
        }
    }
}
