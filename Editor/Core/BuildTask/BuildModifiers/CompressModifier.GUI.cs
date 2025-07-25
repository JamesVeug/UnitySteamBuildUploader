﻿using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class CompressModifier
    {
        private bool m_showFormattedCompressedFileName = false;
        private bool m_showFormattedTargetPathToCompress = false;
        
        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
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
                if (EditorUtils.FormatStringTextField(ref m_compressedFileName, ref m_showFormattedCompressedFileName, ctx))
                {
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Target Path", GUILayout.Width(120));
                if (EditorUtils.FormatStringTextField(ref m_targetPathToCompress, ref m_showFormattedTargetPathToCompress, ctx))
                {
                    isDirty = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Remove old files", GUILayout.Width(120));
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