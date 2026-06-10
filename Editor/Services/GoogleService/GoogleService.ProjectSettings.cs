using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class GoogleService
    {
        private static ReorderableListOfGoogleAppsProjectSettings _reorderableListOfGoogleAppsProjectSettings;
        private static ReorderableListOfGoogleDriveFolders _reorderableListOfGoogleDriveFolders;
        private static ReorderableListOfGoogleChatSpacesProjectSettings _reorderableListOfGoogleChatSpacesProjectSettings;
        private static ReorderableListOfGooglePlayApps _reorderableListOfGooglePlayApps;

        public override bool HasProjectSettingsGUI => true;

        public override void ProjectSettingsGUI()
        {
            base.ProjectSettingsGUI();
            using (new GUILayout.VerticalScope("box"))
            {
                GoogleConfig config = GoogleUIUtils.GetConfig();

                if (_reorderableListOfGoogleAppsProjectSettings == null)
                {
                    _reorderableListOfGoogleAppsProjectSettings = new ReorderableListOfGoogleAppsProjectSettings();
                    _reorderableListOfGoogleAppsProjectSettings.Initialize(config.apps, "Apps",
                        true, (_) =>
                        {
                            GoogleUIUtils.AppPopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                GUILayout.Label("Apps", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Apps are created in the Google Cloud Console.");
                    if (GUILayout.Button("Cloud Console", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://console.cloud.google.com/apis/credentials");
                    }
                }
                GUILayout.Label("See Edit->Preferences->Build Uploader->Services->Google to enter the OAuth2 access Token.");

                if (_reorderableListOfGoogleAppsProjectSettings.OnGUI())
                {
                    GoogleUIUtils.AppPopup.Refresh();
                    GoogleUIUtils.Save();
                }

                GUILayout.Space(20);

                GUILayout.Label("Google Drive Folders", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Folder IDs are taken from the Drive folder URL (drive.google.com/drive/folders/<ID>). Leave blank for the root of My Drive.");
                }

                if (_reorderableListOfGoogleDriveFolders == null)
                {
                    _reorderableListOfGoogleDriveFolders = new ReorderableListOfGoogleDriveFolders();
                    _reorderableListOfGoogleDriveFolders.Initialize(config.driveFolders, "Drive Folders",
                        true, (_) =>
                        {
                            GoogleUIUtils.DriveFolderPopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                if (_reorderableListOfGoogleDriveFolders.OnGUI())
                {
                    GoogleUIUtils.DriveFolderPopup.Refresh();
                    GoogleUIUtils.Save();
                }

                GUILayout.Space(20);

                GUILayout.Label("Google Chat Spaces", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Spaces are referenced by name. See Edit->Preferences->Build Uploader->Services->Google to enter the Webhook URL.");
                }

                if (_reorderableListOfGoogleChatSpacesProjectSettings == null)
                {
                    _reorderableListOfGoogleChatSpacesProjectSettings = new ReorderableListOfGoogleChatSpacesProjectSettings();
                    _reorderableListOfGoogleChatSpacesProjectSettings.Initialize(config.chatSpaces, "Chat Spaces",
                        true, (_) =>
                        {
                            GoogleUIUtils.ChatSpacePopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                if (_reorderableListOfGoogleChatSpacesProjectSettings.OnGUI())
                {
                    GoogleUIUtils.ChatSpacePopup.Refresh();
                    GoogleUIUtils.Save();
                }

                GUILayout.Space(20);

                GUILayout.Label("Google Play Apps", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Package names are the unique application id from the Play Console (e.g. com.example.MyGame).");
                    if (GUILayout.Button("Play Console", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://play.google.com/console/u/0/developers");
                    }
                }
                GUILayout.Label("See Edit->Preferences->Build Uploader->Services->Google to enter the OAuth2 access Token (must include the androidpublisher scope).",
                    EditorStyles.wordWrappedLabel);

                if (_reorderableListOfGooglePlayApps == null)
                {
                    _reorderableListOfGooglePlayApps = new ReorderableListOfGooglePlayApps();
                    _reorderableListOfGooglePlayApps.Initialize(config.playApps, "Play Apps",
                        true, (_) =>
                        {
                            GoogleUIUtils.PlayAppPopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                if (_reorderableListOfGooglePlayApps.OnGUI())
                {
                    GoogleUIUtils.PlayAppPopup.Refresh();
                    GoogleUIUtils.Save();
                }
            }
        }
    }
}
