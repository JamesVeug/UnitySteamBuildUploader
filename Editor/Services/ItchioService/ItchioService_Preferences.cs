using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class ItchioService
    {
        public override void PreferencesGUI()
        {
            GUILayout.Label("Itch.io", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                Itchio.Enabled = GUILayout.Toggle(Itchio.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!Itchio.Enabled))
                {
                    Draw();
                }
            }
        }

        private static void Draw()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = Itchio.Instance.IsInitialized ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("Butler Path:",
                        "The path a folder that contains Butler.exe. Build Uploader uses this to upload builds to Itch.io"), 
                    GUILayout.Width(105));
                GUI.color = temp;


                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://itch.io/docs/butler/installing.html");
                }

                string newPath = GUILayout.TextField(Itchio.ItchioSDKPath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    newPath = EditorUtility.OpenFolderPanel("Itchio Folder", ".", "");
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(Itchio.ItchioSDKPath);
                }

                if (GUILayout.Button("CMD", GUILayout.Width(50)))
                {
                    Itchio.Instance.ShowConsole();
                }

                if (newPath != Itchio.ItchioSDKPath && !string.IsNullOrEmpty(newPath))
                {
                    Itchio.ItchioSDKPath = newPath;
                    Itchio.Instance.Initialize();
                }
            }
        }

        private static void TokenHelp()
        {
            Application.OpenURL("https://itchio.com/settings/tokens");
        }
    }
}