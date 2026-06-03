using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class AppleService
    {
        private static ReorderableListOfAppleApiKeysPreferences _reorderableListOfAppleApiKeysPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("API Keys are created on the App Store Connect dashboard.");
                if (GUILayout.Button("App Store Connect", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://appstoreconnect.apple.com/access/users");
                }
                if (GUILayout.Button("Documentation", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://developer.apple.com/documentation/appstoreconnectapi");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Apple.Enabled = GUILayout.Toggle(Apple.Enabled, "Enabled");
                if (!Apple.Enabled)
                {
                    return;
                }

                if (!Apple.IsRunningOnMac)
                {
                    EditorGUILayout.HelpBox(
                        "Apple uploads require macOS because they go through Xcode's xcrun altool. Assigning a build to a beta group works by API so should work on other platforms. " +
                        "\nYou can still configure keys and apps here, but uploads will not run on this OS.",
                        MessageType.Warning);
                }

                AppleConfig config = AppleUIUtils.GetConfig();
                if (_reorderableListOfAppleApiKeysPreferences == null)
                {
                    _reorderableListOfAppleApiKeysPreferences = new ReorderableListOfAppleApiKeysPreferences();
                    _reorderableListOfAppleApiKeysPreferences.Initialize(config.apiKeys, "API Keys",
                        true, (_) =>
                        {
                            AppleUIUtils.ApiKeyPopup.Refresh();
                            AppleUIUtils.AppPopup.Refresh();
                            AppleUIUtils.BetaGroupPopup.Refresh();
                            AppleUIUtils.Save();
                        });
                }

                if (_reorderableListOfAppleApiKeysPreferences.OnGUI())
                {
                    AppleUIUtils.ApiKeyPopup.Refresh();
                    AppleUIUtils.AppPopup.Refresh();
                    AppleUIUtils.BetaGroupPopup.Refresh();
                    AppleUIUtils.Save();
                }
            }
        }
    }
}
