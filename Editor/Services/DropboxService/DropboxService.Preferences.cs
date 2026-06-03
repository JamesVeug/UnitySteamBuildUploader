using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class DropboxService
    {
        private static ReorderableListOfDropboxAppsPreferences _reorderableListOfDropboxAppsPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Apps are created in the Dropbox App Console. Tokens here are long-lived access tokens.");
                if (GUILayout.Button("App Console", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://www.dropbox.com/developers/apps");
                }
                if (GUILayout.Button("Documentation", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://www.dropbox.com/developers/documentation/http/documentation");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Dropbox.Enabled = GUILayout.Toggle(Dropbox.Enabled, "Enabled");
                if (!Dropbox.Enabled)
                {
                    return;
                }

                DropboxConfig config = DropboxUIUtils.GetConfig();
                if (_reorderableListOfDropboxAppsPreferences == null)
                {
                    _reorderableListOfDropboxAppsPreferences = new ReorderableListOfDropboxAppsPreferences();
                    _reorderableListOfDropboxAppsPreferences.Initialize(config.apps, "Apps",
                        true, (_) =>
                        {
                            DropboxUIUtils.AppPopup.Refresh();
                            DropboxUIUtils.FolderPopup.Refresh();
                            DropboxUIUtils.Save();
                        });
                }

                if (_reorderableListOfDropboxAppsPreferences.OnGUI())
                {
                    DropboxUIUtils.AppPopup.Refresh();
                    DropboxUIUtils.FolderPopup.Refresh();
                    DropboxUIUtils.Save();
                }
            }
        }
    }
}
