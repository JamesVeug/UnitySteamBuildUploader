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
                GUIContent label = new GUIContent("App:", "The Steam App (game) to upload to. This is the App ID you set up in your Steamworks partner account.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= SteamUIUtils.ConfigPopup.DrawPopup(ref m_current, ctx);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Depot:", "The Steam Depot to upload to. Depots are defined in your Steamworks partner account and represent a build target (for example Windows, Mac, Linux).");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= SteamUIUtils.DepotPopup.DrawPopup(m_current, ref m_depot, ctx);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("Branch:", "The Steam Branch to upload to. Branches are defined in your Steamworks partner account and represent different release channels (for example public, beta, alpha)." +
                                                             "\nNOTE: You can not upload to 'default' branch" +
                                                             "\nNOTE: Uploading to 'none' will upload the build to steamworks but not assign to a branch.");
                GUILayout.Label(label, GUILayout.Width(120));
                isDirty |= SteamUIUtils.BranchPopup.DrawPopup(m_current, ref m_destinationBranch, ctx);
            }

            // Tools
            using (new GUILayout.HorizontalScope())
            {
                GUIContent appFileLabel = new GUIContent("Create AppFile:", "If enabled will create a new app file for the current app settings. If false will use an existing app file you can specify.");
                GUILayout.Label(appFileLabel, GUILayout.Width(120));
                isDirty |= CustomToggle.DrawToggle(ref m_createAppFile, GUILayout.Width(20));

                using(new EditorGUI.DisabledScope(m_createAppFile))
                {
                    GUIContent appFilePathLabel = new GUIContent("Path:", "The path to an existing app file to use for the upload. This is only used if 'Create AppFile' is disabled.");
                    GUILayout.Label(appFilePathLabel, GUILayout.Width(35));
                    isDirty |= CustomFilePathTextField.OnGUI(ref m_appFileName, ref m_showFormattedLocalPath, ctx, "vdf");
                    
                    GUIContent overwriteDescLabel = new GUIContent("Overwrite Desc:", "If enabled the chosen appFile will be copied and modified to include the description from the build uploader.");
                    GUILayout.Label(overwriteDescLabel, GUILayout.Width(100));
                    isDirty |= CustomToggle.DrawToggle(ref m_appFileOverwriteDesc, GUILayout.Width(20));
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIContent label = new GUIContent("DepotFile:", "If enabled will create a new depot file for the current depot settings. If false will use an existing depot file you can specify.");
                GUILayout.Label(label, GUILayout.Width(120));
                bool drawToggle = CustomToggle.DrawToggle(ref m_createDepotFile, GUILayout.Width(20));
                isDirty |= drawToggle;
            
                using(new EditorGUI.DisabledScope(m_createDepotFile))
                {
                    GUIContent pathLabel = new GUIContent("Path:", "The path to an existing depot file to use for the upload. This is only used if 'Create DepotFile' is disabled.");
                    GUILayout.Label(pathLabel, GUILayout.Width(35));
                    isDirty |= CustomFilePathTextField.OnGUI(ref m_depotFileName, ref m_showFormattedLocalPath, ctx, "vdf");
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