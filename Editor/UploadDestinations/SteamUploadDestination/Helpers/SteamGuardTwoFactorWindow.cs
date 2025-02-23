using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace Wireframe
{
    /// <summary>
    /// Steam has the option to let you authenticate new logins with a code from the app.
    /// The user will get a notification on their phone when it occurs and can press to allow the build uploader to work
    /// OR they can enter a code manually from the app.
    /// </summary>
    internal class SteamGuardTwoFactorWindow : EditorWindow
    {
        private Action<string> guardCodeCallback;
        private Action<bool> confirmationCallback;
        private string enteredText;
        private bool waitingForCode;

        // [MenuItem("Window/Steam Guard 2 Factor")]
        public static void ShowWindow()
        {
            ShowAsync((S) => { }, (S) => { });
        }

        public static async Task ShowAsync(Action<bool> confirmed, Action<string> codeCallback)
        {
            var window = GetWindow<SteamGuardTwoFactorWindow>();
            window.titleContent = new GUIContent("Steam Guard Two-Factor", BuildUploaderWindow.WindowIcon);
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(400, 320);
            windowPosition.center =
                new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition;

            window.enteredText = "";
            window.waitingForCode = true;
            window.guardCodeCallback = codeCallback;
            window.confirmationCallback = confirmed;
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
                confirmationCallback?.Invoke(false);
            }

            guardCodeCallback = null;
            confirmationCallback = null;
            waitingForCode = false;
        }

        private void OnGUI()
        {
            GUILayout.Label("Looks like you have Two-Factor verification enabled!\n" +
                            "Open the Steam app on your device and do one of the following.");

            GUILayout.Space(10);

            // bold style
            var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("NOTE: Two-Factor authentication does not save.\n" +
                            "So you will need to do this every time you upload a build.\n" +
                            "I am sorry :(", boldStyle);

            GUILayout.Space(20);

            GUILayout.Label("1. Enter the Steam Guard Code");
            using (new EditorGUILayout.HorizontalScope())
            {
                // Draw TextField with large text to emphasise the steam guard
                var largeText = new GUIStyle(GUI.skin.textField) { fontSize = 40 };
                enteredText = GUILayout.TextField(enteredText, largeText).ToUpper();
                using (new EditorGUI.DisabledScope(enteredText.Length < 1))
                {
                    if (GUILayout.Button("Use Code", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        guardCodeCallback?.Invoke(enteredText);
                        waitingForCode = false;
                        Close();
                    }
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("2. Accept the request that appears on your phone.\n" +
                            "Then click the button below.");
            GUILayout.Label("NOTE: If this does not work then try the Guard Code above.");
            if (GUILayout.Button("Confirmed!", GUILayout.Height(50)))
            {
                confirmationCallback?.Invoke(true);
                waitingForCode = false;
                Close();
            }
        }
    }
}