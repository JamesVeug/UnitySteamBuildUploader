using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    internal static class SteamBuildUploaderPreferences
    {
        private static string steamPasswordConfirmation;
        private static bool steamPasswordAssigned = !string.IsNullOrEmpty(SteamSDK.UserPassword);

        [PreferenceItem("Steam Build Uploader")]
        private static void OnPreferencesGUI()
        {
            GUILayout.Label("Preferences for the Steam Build Uploader. Required to log into various networks.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);

            DrawSteamworks();

            GUILayout.Space(20);

            DrawUnityCloud();
        }

        private static void DrawUnityCloud()
        {
            GUILayout.Label("Unity Cloud", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Organization =
                    PasswordField.Draw("Organization:", 105, UnityCloud.Instance.Organization);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Project = PasswordField.Draw("Project:", 105, UnityCloud.Instance.Project);
            }

            using (new GUILayout.HorizontalScope())
            {
                UnityCloud.Instance.Secret = PasswordField.Draw("Secret:", 105, UnityCloud.Instance.Secret);
            }
        }

        private static void DrawSteamworks()
        {
            GUILayout.Label("Steamworks", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = SteamSDK.Instance.IsInitialized ? Color.green : Color.red;
                GUILayout.Label("SteamSDK Path:", GUILayout.Width(105));
                GUI.color = temp;


                if (!SteamSDK.Instance.IsInitialized)
                {
                    if (GUILayout.Button("?", GUILayout.Width(20)))
                    {
                        Application.OpenURL("https://partner.steamgames.com/doc/sdk");
                    }
                }

                string newPath = GUILayout.TextField(SteamSDK.SteamSDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    newPath = EditorUtility.OpenFolderPanel("SteamSDK Folder", ".", "");
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(SteamSDK.SteamSDKPath);
                }

                if (GUILayout.Button("CMD", GUILayout.Width(50)))
                {
                    SteamSDK.Instance.ShowConsole();
                }

                if (newPath != SteamSDK.SteamSDKPath && !string.IsNullOrEmpty(newPath))
                {
                    SteamSDK.SteamSDKPath = newPath;
                    SteamSDK.Instance.Initialize();
                }
            }

            steamPasswordConfirmation = PasswordField.Draw("Confirm Password:", 105, steamPasswordConfirmation);

            if (!steamPasswordAssigned || steamPasswordConfirmation == SteamSDK.UserPassword)
            {
                // Steam username
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserName = PasswordField.Draw("Steam Username:", 105, SteamSDK.UserName);
                }

                // Steam password
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserPassword = PasswordField.Draw("Steam password:", 105, SteamSDK.UserPassword);
                }
            }

            if (GUILayout.Button("Reset"))
            {
                if (EditorUtility.DisplayDialog("Reset Steam Build Uplaoder Preferences",
                        "Are you sure you want to reset all preferences?", "Yes", "No"))
                {
                    SteamSDK.UserName = "";
                    SteamSDK.UserPassword = "";
                    steamPasswordAssigned = false;
                }
            }
        }
    }
}