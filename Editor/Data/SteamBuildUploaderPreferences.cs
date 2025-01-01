using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    internal static class SteamBuildUploaderPreferences
    {
        private static string steamPasswordConfirmation;
        private static bool steamPasswordAssigned = false;

        [PreferenceItem("Steam Build Uploader")]
        private static void OnPreferencesGUI()
        {
            GUILayout.Label("Preferences for the Steam Build Uploader. Required to log into various networks.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);

            GUILayout.Label("Steamworks", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                SteamSDK.Enabled = GUILayout.Toggle(SteamSDK.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!SteamSDK.Enabled))
                {
                    DrawSteamworks();
                }
            }

            GUILayout.Space(20);

            GUILayout.Label("Unity Cloud", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                UnityCloud.Enabled = GUILayout.Toggle(UnityCloud.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!UnityCloud.Enabled))
                {
                    DrawUnityCloud();
                }
            }
        }

        private static void DrawUnityCloud()
        {

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


            if (steamPasswordAssigned || steamPasswordConfirmation == SteamSDK.UserPassword)
            {
                // Steam username
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserName = PasswordField.Draw("Steam Username:", 105, SteamSDK.UserName);
                }

                // Steam password
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserPassword = PasswordField.Draw("Steam Password:", 105, SteamSDK.UserPassword);
                }
            }
            else
            {
                steamPasswordConfirmation = PasswordField.Draw("Password:", 105, steamPasswordConfirmation);
            }

            if (GUILayout.Button("Reset login details"))
            {
                if (EditorUtility.DisplayDialog("Reset Steam Build Uploader Preferences",
                        "Are you sure you want to reset your steam login details?", "Yes", "No"))
                {
                    SteamSDK.UserName = "";
                    SteamSDK.UserPassword = "";
                    steamPasswordConfirmation = "";
                    steamPasswordAssigned = true;
                }
            }
        }
    }
}