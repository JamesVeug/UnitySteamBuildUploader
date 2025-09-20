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
                bool newEnabled = GUILayout.Toggle(SteamSDK.Enabled, "Enabled");
                if (newEnabled != SteamSDK.Enabled)
                {
                    SteamSDK.Enabled = newEnabled;
                }
                
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


                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://partner.steamgames.com/doc/sdk");
                }

                string newPath = GUILayout.TextField(SteamSDK.SteamSDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    var newFolderPath = EditorUtility.OpenFolderPanel("SteamSDK Folder", ".", "");
                    if (!string.IsNullOrEmpty(newFolderPath))
                    {
                        newPath = newFolderPath;
                    }
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(SteamSDK.SteamSDKPath);
                }

                if (GUILayout.Button("CMD", GUILayout.Width(50)))
                {
                    SteamSDK.Instance.ShowConsole();
                }

                if (newPath != SteamSDK.SteamSDKPath)
                {
                    SteamSDK.SteamSDKPath = newPath;
                    SteamSDK.Instance.Initialize();
                }
            }

            // Steam username
            using (new GUILayout.HorizontalScope())
            {
                SteamSDK.UserName = PasswordField.Draw("Steam Username:", "Your Steamworks username used to login", 105, SteamSDK.UserName);
            }
        }
    }
}