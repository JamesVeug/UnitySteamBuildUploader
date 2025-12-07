using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class SteamDRM_BuildModifier
    {
        protected internal override void OnGUIExpanded(ref bool isDirty, Context ctx)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://partner.steamgames.com/doc/features/drm");
                }

                GUILayout.Label(":", GUILayout.Width(10));

                isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_app, ctx, GUILayout.Width(130));

                GUILayout.Label("Flags", GUILayout.Width(40));
                var newFlags = EditorGUILayout.IntField(m_flags, GUILayout.Width(40));
                if (newFlags != m_flags)
                {
                    m_flags = newFlags;
                    isDirty = true;
                }
            }
        }
    }
}