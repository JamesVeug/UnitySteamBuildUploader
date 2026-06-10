using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class XboxService
    {
        private ReorderableListOfXboxAppsPreferences m_appSecretsList;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool newEnabled = GUILayout.Toggle(Xbox.Enabled, "Enabled");
                if (newEnabled != Xbox.Enabled)
                    Xbox.Enabled = newEnabled;

                if (!Xbox.Enabled) return;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(
                        "Enter the Client Secret for each Xbox app registered in Azure AD. " +
                        "Secrets are stored locally and never committed to source control.",
                        EditorStyles.wordWrappedLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Partner Center", GUILayout.Width(120)))
                        Application.OpenURL("https://partner.microsoft.com/dashboard");
                    if (GUILayout.Button("Azure Portal", GUILayout.Width(100)))
                        Application.OpenURL("https://portal.azure.com");
                }

                GUILayout.Space(4);

                XboxConfig config = XboxUIUtils.GetConfig();
                if (config.apps == null || config.apps.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No apps configured yet. Add apps in Project Settings → Build Uploader → Services → Xbox.",
                        MessageType.Info);
                    return;
                }

                if (m_appSecretsList == null)
                {
                    m_appSecretsList = new ReorderableListOfXboxAppsPreferences();
                    m_appSecretsList.Initialize(config.apps, "Apps", false, null);
                }

                m_appSecretsList.OnGUI();
            }
        }
    }
}
