﻿using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class SteamworksService
    {
        public override void PreferencesGUI()
        {
            GUILayout.Label("Steamworks", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                SteamSDK.Enabled = GUILayout.Toggle(SteamSDK.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!SteamSDK.Enabled))
                {
                    DrawSteamworks();
                }
            }
        }

        private static void DrawSteamworks()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = SteamSDK.Instance.IsInitialized ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("SteamSDK Path:",
                        "The path to the SteamSDK folder. Build Uploader uses this to upload builds to Steamworks."), 
                    GUILayout.Width(105));
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


            if (steamPasswordConfirmed)
            {
                // Steam username
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserName = PasswordField.Draw("Steam Username:", "Your Steamworks username used to login", 105, SteamSDK.UserName);
                }

                // Steam password
                using (new GUILayout.HorizontalScope())
                {
                    SteamSDK.UserPassword = PasswordField.Draw("Steam Password:", "Your Steamworks password used to login", 105, SteamSDK.UserPassword);
                }
            }
            else
            {
                steamPasswordConfirmation = PasswordField.Draw("Verify Password:", "Verify your Steamworks password", 105, steamPasswordConfirmation);
                if (steamPasswordConfirmation == SteamSDK.UserPassword)
                {
                    steamPasswordConfirmed = true;
                }
            }

            if (GUILayout.Button("Reset login details"))
            {
                if (EditorUtility.DisplayDialog("Reset Steam Login Preferences",
                        "Are you sure you want to reset your Steam login details?", "Yes", "No"))
                {
                    SteamSDK.UserName = "";
                    SteamSDK.UserPassword = "";
                    steamPasswordConfirmation = "";
                    steamPasswordConfirmed = true; // confirmation password matches
                }
            }
        }
    }
}