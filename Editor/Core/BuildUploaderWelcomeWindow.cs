using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wireframe {
    public class BuildUploaderWelcomeWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        
        [MenuItem("Window/Build Uploader/Welcome", false, 1)]
        public static void ShowWindow()
        {
            BuildUploaderWelcomeWindow window = GetWindow<BuildUploaderWelcomeWindow>();
            window.titleContent = new GUIContent("Welcome to Build Uploader!", Utils.WindowIcon);
            
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(400, Screen.currentResolution.height * 0.5f);
            windowPosition.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition; 
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Need help setting up the Build Uploader?");

            GUIStyle style = GUI.skin.label;
            style.wordWrap = true;
            GUILayout.Label("Check out the Documentation for a step by step guide on how to set up the Build Uploader.", style);

            Links();

            Changes();
        }

        private void Changes()
        {
            GUILayout.Label("Changelog");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            using (new EditorGUI.DisabledScope(true))
            {
                GUIStyle style = GUI.skin.textArea;
                // style.richText = true;
                EditorGUILayout.TextArea(GetChangeLog(), style);
            }

            EditorGUILayout.EndScrollView();
        }

        private string GetChangeLog()
        {
            var iconPath = "Packages/com.veugeljame.builduploader/CHANGELOG.md";
            Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(TextAsset));
            string allText = loadAssetAtPath is TextAsset textAsset ? textAsset.text : "";
            return allText;
        }

        private static void Links()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Documentation"))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader/wiki");
                }
                
                if (GUILayout.Button("Discord"))
                {
                    Application.OpenURL("https://discord.gg/R2UjXB6pQ8");
                }
                
                if (GUILayout.Button("Github"))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader");
                }
                
                if (GUILayout.Button("Support Me"))
                {
                    Application.OpenURL("https://buymeacoffee.com/jamesgamesnz");
                }
                
                if (GUILayout.Button("Report Bug"))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader/issues");
                }
            }
        }
    }
    
    [InitializeOnLoad]
    public class ScriptReloadWatcher
    {
        static ScriptReloadWatcher()
        {
            EditorApplication.delayCall += OnScriptsReloaded;
        }

        private static void OnScriptsReloaded()
        {
            if (!ProjectEditorPrefs.GetBool("BuildUploaderWelcomeWindow"))
            {
                BuildUploaderWelcomeWindow.ShowWindow();
                ProjectEditorPrefs.SetBool("BuildUploaderWelcomeWindow", true);
            }
        }
    }
}