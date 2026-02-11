using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_UploadModifier
    {
        private ReorderableListOfExcludeFileByRegexSelection m_reorderableList = new ReorderableListOfExcludeFileByRegexSelection();

        private void Initialize()
        {
            m_reorderableList.Initialize(m_fileRegexes, ListHeader, m_fileRegexes.Count <= 2);
        }
        
        protected internal override void OnGUIExpanded(ref bool isDirty, Context ctx)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("When To Exclude", GUILayout.Width(120));
                var newCompressionType = (WhenToExclude)EditorGUILayout.EnumPopup(m_WhenToExclude);
                if (m_WhenToExclude != newCompressionType)
                {
                    m_WhenToExclude = newCompressionType;
                    isDirty = true;
                }
            }
            
            isDirty |= m_reorderableList.OnGUI();
        }
    }
}