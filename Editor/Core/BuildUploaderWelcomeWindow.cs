﻿using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wireframe {
    public class BuildUploaderWelcomeWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        
        [MenuItem("Window/Build Uploader/Welcome", false, 0)]
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

            GUILayout.Label("Changelog");
            using (new EditorGUILayout.VerticalScope("box"))
            {
                Changes();
            }
        }

        private void Changes()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var path = "Packages/com.veugeljame.builduploader/CHANGELOG.md";
            Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            string allText = loadAssetAtPath is TextAsset textAsset ? textAsset.text : "";
            string[] lines = allText.Split('\n');
            
            // Draw allText as markdown
            // # is header
            // - bullet point

            for (int i = 0; i < lines.Length; i++)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.richText = true;
                
                // Get text and style based on the line content
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    line = "";
                }
                else if (line.StartsWith("##"))
                {
                    // Sub-Header
                    line = line.Substring(2).Trim();
                    style = new GUIStyle(EditorStyles.boldLabel);
                    style.fontSize = 16;
                }
                else if (line.StartsWith("#"))
                {
                    // Header
                    line = line.Substring(1).Trim();
                    style = new GUIStyle(EditorStyles.boldLabel);
                    style.fontSize = 24;
                }
                else if (line.Trim().StartsWith("-"))
                {
                    // Bullet point
                    int indents = Mathf.CeilToInt(line.IndexOf('-') / 2f);

                    int artificialIndent = (indents + 1) * 10;
                    line = new string(' ', artificialIndent) + line.Trim().Substring(1).Trim();
                    // line = line.Substring(1).Trim();
                    // GUILayout.Label($"- {bulletText}");
                }
                
                // replace **XXXX** with <b>XXXX</b>
                int boldStartIndex = line.IndexOf("**");
                while (boldStartIndex != -1)
                {
                    int boldEndIndex = line.IndexOf("**", boldStartIndex + 2);
                    if (boldEndIndex == -1)
                    {
                        break; // No closing bold found
                    }

                    string boldText = line.Substring(boldStartIndex + 2, boldEndIndex - boldStartIndex - 2);
                    string coloredBoldText = $"<b>{boldText}</b>";
                    line = line.Replace($"**{boldText}**", coloredBoldText);
                    
                    boldStartIndex = line.IndexOf("**", boldEndIndex + 2);
                }
                
                
                // replace `XXXX` with colorization
                int startIndex = line.IndexOf('`');
                while (startIndex != -1)
                {
                    int endIndex = line.IndexOf('`', startIndex + 1);
                    if (endIndex == -1)
                    {
                        break; // No closing backtick found
                    }

                    string codeSnippet = line.Substring(startIndex + 1, endIndex - startIndex - 1);
                    string coloredSnippet = $"<color=yellow>{codeSnippet}</color>";
                    line = line.Replace($"`{codeSnippet}`", coloredSnippet);
                    
                    startIndex = line.IndexOf('`', endIndex + 1);
                }
                
                // replace [docs](XXX) with a button
                int linkStartIndex = line.IndexOf('[');
                if (linkStartIndex != -1)
                {
                    int linkEndIndex = line.IndexOf(']', linkStartIndex + 1);
                    if (linkEndIndex > -1)
                    {

                        int urlStartIndex = line.IndexOf('(', linkEndIndex + 1);
                        if (urlStartIndex > -1)
                        {

                            int urlEndIndex = line.IndexOf(')', urlStartIndex + 1);
                            if (urlEndIndex > -1)
                            {

                                string linkText = line.Substring(linkStartIndex + 1, linkEndIndex - linkStartIndex - 1);
                                string url = line.Substring(urlStartIndex + 1, urlEndIndex - urlStartIndex - 1);

                                line = line.Replace($"[{linkText}]({url})",
                                    $"<b><color=blue><u>{linkText}</u></color></b>");
                                if (GUILayout.Button(line, style))
                                {
                                    Application.OpenURL(url);
                                }
                                continue;
                            }
                        }
                    }
                }
                
                
                EditorGUILayout.LabelField(line, style);
            }
            
            

            EditorGUILayout.EndScrollView();
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