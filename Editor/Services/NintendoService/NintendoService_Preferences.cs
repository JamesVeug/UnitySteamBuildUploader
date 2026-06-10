using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class NintendoService
    {
        public static bool DeleteAuthoringFilesDuringCleanup
        {
            get => EditorPrefs.GetBool("BuildUploader_DeleteNintendoAuthoringFilesDuringCleanup", true);
            set => EditorPrefs.SetBool("BuildUploader_DeleteNintendoAuthoringFilesDuringCleanup", value);
        }

        public override void PreferencesGUI()
        {
            base.PreferencesGUI();
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool newEnabled = GUILayout.Toggle(NintendoSDK.Enabled, "Enabled");
                if (newEnabled != NintendoSDK.Enabled)
                {
                    NintendoSDK.Enabled = newEnabled;
                }

                using (new EditorGUI.DisabledScope(!NintendoSDK.Enabled))
                {
                    DrawNintendoSDK();
                }

                EditorGUILayout.Space();

                GUILayout.Label("Options:");
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        new GUIContent("Delete authoring files after uploading",
                            "If enabled, generated authoring command files will be deleted when an upload completes."),
                        GUILayout.Width(220));

                    bool delete = DeleteAuthoringFilesDuringCleanup;
                    bool newDelete = EditorGUILayout.Toggle(delete);
                    if (newDelete != DeleteAuthoringFilesDuringCleanup)
                    {
                        DeleteAuthoringFilesDuringCleanup = newDelete;
                    }
                }
            }
        }

        private static void DrawNintendoSDK()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = NintendoSDK.Instance.IsInitialized ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("Nintendo SDK Path:",
                        "The path to the Nintendo SDK folder. Build Uploader uses this to upload builds to the Nintendo Developer Center."),
                    GUILayout.Width(135));
                GUI.color = temp;


                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://developer.nintendo.com/");
                }

                string newPath = GUILayout.TextField(NintendoSDK.NintendoSDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    var newFolderPath = EditorUtility.OpenFolderPanel("Nintendo SDK Folder", ".", "");
                    if (!string.IsNullOrEmpty(newFolderPath))
                    {
                        newPath = newFolderPath;
                    }
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(NintendoSDK.NintendoSDKPath);
                }

                if (newPath != NintendoSDK.NintendoSDKPath)
                {
                    NintendoSDK.NintendoSDKPath = newPath;
                    NintendoSDK.Instance.Initialize();
                }
            }

            // Nintendo developer username
            using (new GUILayout.HorizontalScope())
            {
                NintendoSDK.UserName = PasswordField.Draw("Developer Username:", "Your Nintendo Developer Center username used to authorise uploads", 135, NintendoSDK.UserName, labelIsRedIfEmpty:true);
            }

            EditorGUILayout.Space();

            GUILayout.Label("Team Notification (optional)", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Webhook URL:",
                        "URL of the internal team relay/webhook that the Nintendo Notify action posts to."),
                    GUILayout.Width(135));
                string newWebhook = GUILayout.TextField(NintendoSDK.NotificationWebhook);
                if (newWebhook != NintendoSDK.NotificationWebhook)
                {
                    NintendoSDK.NotificationWebhook = newWebhook;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                NintendoSDK.NotificationToken = PasswordField.Draw("Webhook Token:",
                    "Bearer token sent with the team notification webhook request (optional).",
                    135, NintendoSDK.NotificationToken);
            }
        }
    }
}
