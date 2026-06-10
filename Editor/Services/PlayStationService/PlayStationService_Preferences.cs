using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class PlayStationService
    {
        public static bool DeleteAuthoringFilesDuringCleanup
        {
            get => EditorPrefs.GetBool("BuildUploader_DeletePlayStationAuthoringFilesDuringCleanup", true);
            set => EditorPrefs.SetBool("BuildUploader_DeletePlayStationAuthoringFilesDuringCleanup", value);
        }

        public override void PreferencesGUI()
        {
            base.PreferencesGUI();
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool newEnabled = GUILayout.Toggle(PlayStationSDK.Enabled, "Enabled");
                if (newEnabled != PlayStationSDK.Enabled)
                {
                    PlayStationSDK.Enabled = newEnabled;
                }

                using (new EditorGUI.DisabledScope(!PlayStationSDK.Enabled))
                {
                    DrawPlayStationSDK();
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

        private static void DrawPlayStationSDK()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = PlayStationSDK.Instance.IsInitialized ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("PlayStation SDK Path:",
                        "The path to the PlayStation SDK root folder (the folder containing host_tools/bin). Build Uploader uses this to locate the publishing tool used to upload builds to PlayStation Partners."),
                    GUILayout.Width(160));
                GUI.color = temp;

                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://partners.playstation.net/");
                }

                string newPath = GUILayout.TextField(PlayStationSDK.PlayStationSDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    var newFolderPath = EditorUtility.OpenFolderPanel("PlayStation SDK Folder", ".", "");
                    if (!string.IsNullOrEmpty(newFolderPath))
                    {
                        newPath = newFolderPath;
                    }
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(PlayStationSDK.PlayStationSDKPath);
                }

                if (newPath != PlayStationSDK.PlayStationSDKPath)
                {
                    PlayStationSDK.PlayStationSDKPath = newPath;
                    PlayStationSDK.Instance.Initialize();
                }
            }

            // PlayStation developer username
            using (new GUILayout.HorizontalScope())
            {
                PlayStationSDK.UserName = PasswordField.Draw("Developer Username:", "Your PlayStation Partners developer username used to authorise uploads", 160, PlayStationSDK.UserName, labelIsRedIfEmpty:true);
            }

            // PlayStation developer password / passphrase (optional - some publishing tools take a stored credential file instead)
            using (new GUILayout.HorizontalScope())
            {
                PlayStationSDK.Password = PasswordField.Draw("Developer Password:", "Your PlayStation Partners developer password. Leave blank if your local SDK has already cached credentials.", 160, PlayStationSDK.Password);
            }
        }
    }
}
