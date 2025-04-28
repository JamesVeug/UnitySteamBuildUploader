using UnityEngine;

namespace Wireframe
{
    public partial class SteamUploadDestination
    {
        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            // Config
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("App:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_current);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depot:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.DepotPopup.DrawPopup(m_current, ref m_depot);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch:", GUILayout.Width(120));
                isDirty |= SteamUIUtils.BranchPopup.DrawPopup(m_current, ref m_destinationBranch);
            }

            // Tools
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create AppFile:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createAppFile);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create DepotFile:", GUILayout.Width(120));
                bool drawToggle = CustomToggle.DrawToggle(ref m_createDepotFile);
                isDirty |= drawToggle;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Upload To Steam:", GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_uploadToSteam);
            }
        }

        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth)
        {
            isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_current);
            isDirty |= SteamUIUtils.DepotPopup.DrawPopup(m_current, ref m_depot);
            isDirty |= SteamUIUtils.BranchPopup.DrawPopup(m_current, ref m_destinationBranch);
        }
    }
}