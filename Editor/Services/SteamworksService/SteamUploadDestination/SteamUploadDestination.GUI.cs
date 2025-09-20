using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class SteamUploadDestination
    {
        private bool m_showFormattedLocalPath = false;
        
        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            // Config
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_current, ctx);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depot:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.DepotPopup.DrawPopup(m_current, ref m_depot, ctx);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.BranchPopup.DrawPopup(m_current, ref m_destinationBranch, ctx);
            }

            // Tools
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create AppFile:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createAppFile, GUILayout.Width(50));

                using(new EditorGUI.DisabledScope(m_createAppFile))
                {
                    GUILayout.Label("File Name:", GUILayout.Width(70));
                    if (EditorUtils.FormatStringTextField(ref m_appFileName, ref m_showFormattedLocalPath, ctx))
                    {
                        isDirty = true;
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create DepotFile:", GUILayout.Width(120));
                bool drawToggle = CustomToggle.DrawToggle(ref m_createDepotFile, GUILayout.Width(50));
                isDirty |= drawToggle;
            
                using(new EditorGUI.DisabledScope(m_createDepotFile))
                {
                    GUILayout.Label("File Name:", GUILayout.Width(70));
                    if (EditorUtils.FormatStringTextField(ref m_depotFileName, ref m_showFormattedLocalPath, ctx))
                    {
                        isDirty = true;
                    }
                }
            }
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_current, ctx);
            isDirty |= SteamUIUtils.DepotPopup.DrawPopup(m_current, ref m_depot, ctx);
            isDirty |= SteamUIUtils.BranchPopup.DrawPopup(m_current, ref m_destinationBranch, ctx);
        }
    }
}