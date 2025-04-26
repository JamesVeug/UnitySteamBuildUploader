using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class CompressModifier
    {
        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Compression Type", GUILayout.Width(120));
                var newCompressionType = (CompressionType)EditorGUILayout.EnumPopup(m_compressionType);
                if (m_compressionType != newCompressionType)
                {
                    m_compressionType = newCompressionType;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Compressed Name", GUILayout.Width(120));
                var newFileName = EditorGUILayout.TextField(m_compressedFileName);
                if (m_compressedFileName != newFileName)
                {
                    m_compressedFileName = newFileName;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Target Path", GUILayout.Width(120));
                var newSuPath = EditorGUILayout.TextField(m_subPathToCompress);
                if (m_subPathToCompress != newSuPath)
                {
                    m_subPathToCompress = newSuPath;
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Remove Old files", GUILayout.Width(120));
                var newRemoveContent = EditorGUILayout.Toggle(m_removeContentAfterCompress, GUILayout.Width(20));
                if (m_removeContentAfterCompress != newRemoveContent)
                {
                    m_removeContentAfterCompress = newRemoveContent;
                    isDirty = true;
                }
            }
        }
    }
}