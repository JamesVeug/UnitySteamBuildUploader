﻿using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


namespace Wireframe
{
    /// <summary>
    /// SteamGuard has an option to email/text you a verification code for new logins.
    /// </summary>
    internal class SteamGuardWindow : EditorWindow
    {
        private Action<string> guardCodeCallback;
        private string enteredText;
        private bool waitingForCode;

        // [MenuItem("Window/Steam Guard")]
        public static void ShowWindow()
        {
            ShowAsync((S) => { });
        }


        public static async Task ShowAsync(Action<string> codeCallback)
        {
            var window = CreateWindow<SteamGuardWindow>();
            window.titleContent = new GUIContent("Steam Guard Verification", Utils.WindowIcon);
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(300, 200);
            windowPosition.center =
                new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition;

            window.enteredText = "";
            window.guardCodeCallback = codeCallback;
            window.waitingForCode = true;
            while (window.waitingForCode)
            {
                await Task.Delay(100);
            }
        }

        private void OnEnable()
        {
            // Reset the entered text - fixes null when loading unity with this popup open
            enteredText = "";
        }

        private void OnDisable()
        {
            if (waitingForCode)
            {
                guardCodeCallback?.Invoke(null);
            }

            guardCodeCallback = null;
            waitingForCode = false;
        }

        private void OnGUI()
        {
            GUILayout.Label("Looks like you need to verify Steam Guard!\n" +
                            "Enter the Steam Guard code below to continue.");


            // Draw TextField with large text to emphasise the steam guard
            var largeText = new GUIStyle(GUI.skin.textField) { fontSize = 40 };
            enteredText = GUILayout.TextField(enteredText, largeText).ToUpper();

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(enteredText.Length < 1))
            {
                if (GUILayout.Button("Retry login", GUILayout.Height(50)))
                {
                    guardCodeCallback?.Invoke(enteredText.Trim());
                    waitingForCode = false;
                    Close();
                }
            }
        }
    }
}