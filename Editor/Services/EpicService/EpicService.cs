using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EpicService : AService
    {
        public EpicService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Github.Enabled)
            {
                reason = "Github is not enabled in Preferences";
                return false;
            }

            if (string.IsNullOrEmpty(Github.Token))
            {
                reason = "Github Token credentials are not set in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override void ProjectSettingsGUI()
        {

        }

        public override void PreferencesGUI()
        {
            GUILayout.Label("EpicGames", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool newEnabled = GUILayout.Toggle(Epic.Enabled, "Enabled");
                if (newEnabled != Epic.Enabled)
                {
                    Epic.Enabled = newEnabled;
                }

                using (new EditorGUI.DisabledScope(!Epic.Enabled))
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
                GUILayout.Label(new GUIContent("EpicGamesSDK Path:",
                        "The path to the EpicGamesSDK folder. Build Uploader uses this to upload builds to EpicGames."),
                    GUILayout.Width(105));
                GUI.color = temp;


                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://partner.steamgames.com/doc/sdk");
                }

                string newPath = GUILayout.TextField(Epic.SDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    var newFolderPath = EditorUtility.OpenFilePanel("EpicGamesSDK Folder", ".", "exe");
                    if (!string.IsNullOrEmpty(newFolderPath))
                    {
                        newPath = newFolderPath;
                    }
                }

                if (newPath != Epic.SDKPath)
                {
                    Epic.SDKPath = newPath;
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(Epic.SDKPath);
                }
            }
        }
    }
}