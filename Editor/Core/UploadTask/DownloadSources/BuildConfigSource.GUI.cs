using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfigSource
    {
        public override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Build Config:", GUILayout.Width(120));
                isDirty |= BuildConfigsUIUtils.BuildConfigsPopup.DrawPopup(ref m_BuildConfig);
            }
        }

        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= BuildConfigsUIUtils.BuildConfigsPopup.DrawPopup(ref m_BuildConfig);
        }
    }
}