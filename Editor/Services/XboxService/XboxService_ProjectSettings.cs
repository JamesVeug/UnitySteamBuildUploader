using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class XboxService
    {
        private ReorderableListOfXboxApps m_appList;

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                XboxConfig config = XboxUIUtils.GetConfig();

                GUILayout.Label("Apps", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(
                        "Add one entry per game registered in Microsoft Partner Center. " +
                        "Product ID, Tenant ID, and Client ID are safe to commit. " +
                        "Enter the Client Secret in Edit → Preferences → Build Uploader → Services → Xbox.",
                        EditorStyles.wordWrappedLabel);
                }

                GUILayout.Space(4);

                if (GUILayout.Button("Add App", GUILayout.Width(100)))
                {
                    XboxConfig.XboxApp app = new XboxConfig.XboxApp();
                    List<XboxConfig.XboxApp> apps = config.apps;
                    app.Id = apps.Count > 0 ? apps.Max(a => a.Id) + 1 : 1;
                    apps.Add(app);
                    XboxUIUtils.Save();
                    XboxUIUtils.AppPopup.Refresh();
                    m_appList = null;
                }

                GUILayout.Space(4);

                if (m_appList == null)
                {
                    m_appList = new ReorderableListOfXboxApps();
                    m_appList.Initialize(config.apps, "Apps", true, (_) =>
                    {
                        XboxUIUtils.AppPopup.Refresh();
                        XboxUIUtils.Save();
                    });
                }

                if (m_appList.OnGUI())
                {
                    XboxUIUtils.AppPopup.Refresh();
                    XboxUIUtils.Save();
                }

                GUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "Product ID — Microsoft Store App identity (e.g. \"1ABDEEFGHI2\"). " +
                    "Found in Partner Center → App Management → App identity.\n" +
                    "Tenant ID & Client ID — From the Azure AD app registration used for API access. " +
                    "All three values are safe to commit to source control.",
                    MessageType.Info);
            }
        }
    }
}
