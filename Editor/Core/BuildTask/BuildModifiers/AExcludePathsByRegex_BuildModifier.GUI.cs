using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_BuildModifier
    {
        protected internal override void OnGUIExpanded(ref bool isDirty)
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