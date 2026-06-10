using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class DropboxService
    {
        private static ReorderableListOfDropboxAppsProjectSettings _reorderableListOfDropboxAppsProjectSettings;
        private static ReorderableListOfDropboxFolders _reorderableListOfDropboxFolders;

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            base.ProjectSettingsGUI();
            using (new GUILayout.VerticalScope("box"))
            {
                DropboxConfig config = DropboxUIUtils.GetConfig();

                if (_reorderableListOfDropboxAppsProjectSettings == null)
                {
                    _reorderableListOfDropboxAppsProjectSettings = new ReorderableListOfDropboxAppsProjectSettings();
                    _reorderableListOfDropboxAppsProjectSettings.Initialize(config.apps, "Apps",
                        true, (_) =>
                        {
                            DropboxUIUtils.AppPopup.Refresh();
                            DropboxUIUtils.Save();
                        });
                }

                GUILayout.Label("Apps", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Apps are created in the Dropbox App Console.");
                    if (GUILayout.Button("App Console", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://www.dropbox.com/developers/apps");
                    }
                }
                GUILayout.Label("See Edit->Preferences->Build Uploader->Services->Dropbox to enter the access Token.");

                if (_reorderableListOfDropboxAppsProjectSettings.OnGUI())
                {
                    DropboxUIUtils.AppPopup.Refresh();
                    DropboxUIUtils.Save();
                }

                GUILayout.Space(20);

                GUILayout.Label("Dropbox Folders", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Folder paths are relative to the app/account root (e.g. /Builds). Leave blank for the root.");
                }

                if (_reorderableListOfDropboxFolders == null)
                {
                    _reorderableListOfDropboxFolders = new ReorderableListOfDropboxFolders();
                    _reorderableListOfDropboxFolders.Initialize(config.folders, "Folders",
                        true, (_) =>
                        {
                            DropboxUIUtils.FolderPopup.Refresh();
                            DropboxUIUtils.Save();
                        });
                }

                if (_reorderableListOfDropboxFolders.OnGUI())
                {
                    DropboxUIUtils.FolderPopup.Refresh();
                    DropboxUIUtils.Save();
                }
            }
        }
    }
}
