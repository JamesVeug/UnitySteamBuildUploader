using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class GoogleService
    {
        private static ReorderableListOfGoogleAppsPreferences _reorderableListOfGoogleAppsPreferences;
        private static ReorderableListOfGoogleChatSpacesPreferences _reorderableListOfGoogleChatSpacesPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Apps are created in Google Cloud Console. Tokens here are OAuth2 access tokens used by Google Drive.");
                if (GUILayout.Button("Cloud Console", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://console.cloud.google.com/apis/credentials");
                }
                if (GUILayout.Button("OAuth Playground", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://developers.google.com/oauthplayground/");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Google.Enabled = GUILayout.Toggle(Google.Enabled, "Enabled");
                if (!Google.Enabled)
                {
                    return;
                }

                GoogleConfig config = GoogleUIUtils.GetConfig();
                if (_reorderableListOfGoogleAppsPreferences == null)
                {
                    _reorderableListOfGoogleAppsPreferences = new ReorderableListOfGoogleAppsPreferences();
                    _reorderableListOfGoogleAppsPreferences.Initialize(config.apps, "Apps",
                        true, (_) =>
                        {
                            GoogleUIUtils.AppPopup.Refresh();
                            GoogleUIUtils.DriveFolderPopup.Refresh();
                            GoogleUIUtils.ChatSpacePopup.Refresh();
                            GoogleUIUtils.PlayAppPopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                if (_reorderableListOfGoogleAppsPreferences.OnGUI())
                {
                    GoogleUIUtils.AppPopup.Refresh();
                    GoogleUIUtils.DriveFolderPopup.Refresh();
                    GoogleUIUtils.ChatSpacePopup.Refresh();
                    GoogleUIUtils.Save();
                }

                GUILayout.Space(10);
                GUILayout.Label("Google Chat Spaces", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Webhook URLs are issued per-space from the Apps & integrations menu in a Google Chat space.");
                    if (GUILayout.Button("How to", GUILayout.Width(150)))
                    {
                        Application.OpenURL("https://developers.google.com/workspace/chat/quickstart/webhooks");
                    }
                }

                if (_reorderableListOfGoogleChatSpacesPreferences == null)
                {
                    _reorderableListOfGoogleChatSpacesPreferences = new ReorderableListOfGoogleChatSpacesPreferences();
                    _reorderableListOfGoogleChatSpacesPreferences.Initialize(config.chatSpaces, "Chat Spaces",
                        true, (_) =>
                        {
                            GoogleUIUtils.ChatSpacePopup.Refresh();
                            GoogleUIUtils.Save();
                        });
                }

                if (_reorderableListOfGoogleChatSpacesPreferences.OnGUI())
                {
                    GoogleUIUtils.ChatSpacePopup.Refresh();
                    GoogleUIUtils.Save();
                }
            }
        }
    }
}
