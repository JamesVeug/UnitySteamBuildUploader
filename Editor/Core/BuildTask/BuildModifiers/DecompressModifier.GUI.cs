using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class DecompressModifier
    {
        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Decompression Type", GUILayout.Width(120));
                var newCompressionType = (DecompressionType)EditorGUILayout.EnumPopup(m_decompressionType);
                if (m_decompressionType != newCompressionType)
                {
                    m_decompressionType = newCompressionType;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("File Path", GUILayout.Width(120));
                var newFileName = EditorGUILayout.TextField(m_filePath);
                if (m_filePath != newFileName)
                {
                    m_filePath = newFileName;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Target Path", GUILayout.Width(120));
                var newSuPath = EditorGUILayout.TextField(m_targetPathToCompress);
                if (m_targetPathToCompress != newSuPath)
                {
                    m_targetPathToCompress = newSuPath;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Remove old file", GUILayout.Width(120));
                var newRemoveContent = EditorGUILayout.Toggle(m_removeCompressedFile, GUILayout.Width(20));
                if (m_removeCompressedFile != newRemoveContent)
                {
                    m_removeCompressedFile = newRemoveContent;
                    isDirty = true;
                }
            }
        }
    }
}