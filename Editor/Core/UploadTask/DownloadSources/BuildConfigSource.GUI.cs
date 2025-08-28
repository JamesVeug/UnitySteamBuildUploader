using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfigSource
    {
        public override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= BuildConfigsUIUtils.BuildConfigsPopup.DrawPopup(ref m_BuildConfig);
            
            bool newCleanBuild = GUILayout.Toggle(m_CleanBuild, "Clean Build");
            if (newCleanBuild != m_CleanBuild)
            {
                m_CleanBuild = newCleanBuild;
                isDirty = true;
            }
        }
        
        public override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Build Config:", GUILayout.Width(120));
                isDirty |= BuildConfigsUIUtils.BuildConfigsPopup.DrawPopup(ref m_BuildConfig);
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUIContent cleanBuildContent = new GUIContent("Clean Build:", "If enabled, the build folder will be deleted before building. This ensures a fresh build but may increase build time.");
                GUILayout.Label(cleanBuildContent, GUILayout.Width(120));
                bool newCleanBuild = GUILayout.Toggle(m_CleanBuild, "");
                if (newCleanBuild != m_CleanBuild)
                {
                    m_CleanBuild = newCleanBuild;
                    isDirty = true;
                }
            }
            
        }
    }
}