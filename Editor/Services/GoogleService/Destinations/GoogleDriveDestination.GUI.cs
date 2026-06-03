using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class GoogleDriveDestination
    {
        private bool m_showFormattedFileName = Preferences.DefaultShowFormattedTextToggle;

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= GoogleUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(120));
            isDirty |= GoogleUIUtils.DriveFolderPopup.DrawPopup(ref m_folder, m_context, GUILayout.Width(120));

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
                Application.OpenURL("https://developers.google.com/drive/api/reference/rest/v3/files/create");
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= GoogleUIUtils.AppPopup.DrawPopup(ref m_app, m_context, GUILayout.Width(200));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Folder:", GUILayout.Width(120));
                isDirty |= GoogleUIUtils.DriveFolderPopup.DrawPopup(ref m_folder, m_context, GUILayout.Width(200));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("File Name:", "The name the uploaded file will appear as on Drive.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= EditorUtils.FormatStringTextField(ref m_fileNameFormat, ref m_showFormattedFileName, m_context);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip Contents:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_zipContents);
            }
        }

        public override string Summary()
        {
            string app = m_app != null ? m_app.Name : "<no app>";
            string folder = m_folder != null ? m_folder.Name : "My Drive";
            return $"Google Drive: {app} → {folder}";
        }
    }
}
