using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_UploadModifier
    {
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