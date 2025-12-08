using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wireframe {
    public class BuildUploaderWelcomeWindow : EditorWindow
    {
        private class VersionData
        {
            public string title;
            public bool foldoutOpen;
            public string[] lines;
            
            public VersionData(string title, string[] lines)
            {
                this.title = title;
                this.lines = lines;
                foldoutOpen = false;
            }
        }

        private GUIStyle headerLabelStyle;
        private GUIStyle sectionLabelStyle;
        private GUIStyle sectionFoldoutStyle;
        private Vector2 scrollPosition;
        
        private List<VersionData> parsedChangeLog;
        
        [MenuItem("Window/Build Uploader/Welcome", false, 0)]
        public static void ShowWindow()
        {
            BuildUploaderWelcomeWindow window = GetWindow<BuildUploaderWelcomeWindow>();
            window.titleContent = new GUIContent("Welcome to Build Uploader!", Utils.WindowIcon);
            
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(Screen.currentResolution.width * 0.5f, Screen.currentResolution.height * 0.75f);
            windowPosition.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition; 
            window.Show();
        }

        private void OnGUI()
        {
            Parse();
            DrawLinks();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label(Utils.WindowLargeIcon, headerLabelStyle);
            GUILayout.Label("Build Uploader", headerLabelStyle);
            
            GUILayout.Label("Welcome to the Build Uploader!");
            GUILayout.Label("This tool is designed to make it easy to make a build and upload it to all kinds of services.");
            
            
            GUILayout.Label("- Want more information? See the Documentation!");
            GUILayout.Label("- Want to talk to the Dev or others that use the Build Uploader? Join our Discord!");
            GUILayout.Label("- Want to see the source code or view in progress changes/fixes? Go to Github!");
            GUILayout.Label("- Want to ask questions or report a bug or suggest changes? Report Bug/Suggest Feature!");
            GUILayout.Label("- Want to support the project? Check it out on the Unity Asset Store or press Support Me!");
            
            EditorGUILayout.Space(20);
            
            DrawSetupCheckList();

            EditorGUILayout.Space();
            
            GUILayout.Label("Changelog", sectionLabelStyle);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                DrawChanges();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupCheckList()
        {
            bool oneServiceReadyToBuild = InternalUtils.AllServices().Any(a => a.IsReadyToStartBuild(out _));
            bool oneServiceProjectSettingsSetup = InternalUtils.AllServices().Any(a =>
            {
                bool success = a.IsReadyToStartBuild(out _) && a.IsProjectSettingsSetup();
                return success;
            });
            bool oneUploadProfileSetup = UploadProfilesExist();
            
            bool allComplete = oneServiceReadyToBuild && oneServiceProjectSettingsSetup && oneUploadProfileSetup;
            
            bool show = EditorPrefs.GetBool("BuildUploader_showHowToSetup", true);
            bool newShow = EditorGUILayout.Foldout(show, new GUIContent("Setup checklist", SuccessIcon(allComplete, true)), sectionFoldoutStyle);
            if (newShow != show)
            {
                EditorPrefs.SetBool("BuildUploader_showHowToSetup", newShow);
            }
            

            GUILayout.Label("Need more help setting up the Build Uploader?");
            
            if (newShow)
            {
                GUIStyle header = new GUIStyle(GUI.skin.label);
                header.fontStyle = FontStyle.Bold;

                GUIStyle scopeIndex = new GUIStyle(GUIStyle.none);
                scopeIndex.margin.left = 10;

                GUIStyle mainScope = "box";
                mainScope.margin.left = 10;
                
                
                using (new EditorGUILayout.VerticalScope(mainScope))
                {
                    GUILayout.Label("Setup Preferences (Edit->Preferences)", header);
                    GUILayout.Label("These are settings for your project and not shared with anyone.");

                    using (new EditorGUILayout.VerticalScope(scopeIndex))
                    {
                        GUILayout.Label($"\nBuild Uploader -> General");
                        DrawCheckList("Change Cached Builds to a smaller path. eg: C:/CachedBuilds",
                            null, !Preferences.CacheFolderPath.Equals(Preferences.DefaultCacheFolder));

                        GUILayout.Label($"\nBuild Uploader -> Services");
                        DrawCheckList("Enable and enter credentials for all services you want to use",
                            "Enable Steamworks, download and install SteamSDK and enter your username.",
                            oneServiceReadyToBuild);
                    }

                    GUILayout.Label("\nSetup Project Settings (Edit->Project Settings)", header);
                    GUILayout.Label("These are specific to your project and will be shared with anyone with access to your source code.");

                    using (new EditorGUILayout.VerticalScope(scopeIndex))
                    {
                        GUILayout.Label($"\nBuild Uploader -> Services");
                        DrawCheckList("Enter settings for all Services you want to use",
                            "For Steamworks add a new App for your game and any branches you want to use.",
                            oneServiceProjectSettingsSetup);
                    }

                    GUILayout.Label("\nSetup Upload Profile (Window -> Build Uploader -> Open Window)", header);
                    using (new EditorGUILayout.VerticalScope(scopeIndex))
                    {
                        DrawCheckList(
                            "Create an Upload Config so you can make a build and upload it to a service of your choosing.",
                            "", oneUploadProfileSetup);
                    }
                }
            }
        }

        private bool UploadProfilesExist()
        {
            string path = WindowUploadTab.UploadProfilePath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }
            
            string[] files = Directory.GetFiles(path, "*.json");
            return files.Length > 0;
        }

        private Texture2D SuccessIcon(bool success, bool big)
        {
            if (big)
            {
                return success ? Utils.CheckIcon : Utils.CrossIcon;
            }
            return success ? Utils.CheckIconSmall : Utils.CrossIconSmall;
        }
        
        private void DrawCheckList(string text, string example, bool isComplete)
        {
            GUIStyle exampleStyle = new GUIStyle(GUI.skin.label);
            exampleStyle.fontStyle = FontStyle.Italic;
            exampleStyle.wordWrap = true;
            exampleStyle.richText = true;
            exampleStyle.onNormal.textColor = Color.black;
            
            GUILayout.Label(new GUIContent(text, SuccessIcon(isComplete, false)));
            if (!string.IsNullOrEmpty(example))
            {
                GUILayout.Label($"\tExample: {example}", exampleStyle);
            }
        }

        private void Parse()
        {
            if (parsedChangeLog != null)
            {
                return;
            }
            
            headerLabelStyle = new GUIStyle(GUI.skin.label);
            headerLabelStyle.wordWrap = true;
            headerLabelStyle.alignment = TextAnchor.MiddleCenter;
            headerLabelStyle.fontStyle = FontStyle.Bold;
            headerLabelStyle.fontSize = 24;
            
            sectionLabelStyle = new GUIStyle(GUI.skin.label);
            sectionLabelStyle.wordWrap = true;
            sectionLabelStyle.alignment = TextAnchor.MiddleLeft;
            sectionLabelStyle.fontStyle = FontStyle.Normal;
            sectionLabelStyle.fontSize = 18;

            sectionFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            sectionFoldoutStyle.fontSize = 16;
            
            var path = "Packages/com.veugeljame.builduploader/CHANGELOG.md";
            Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            string allText = loadAssetAtPath is TextAsset textAsset ? textAsset.text : "";
            string[] lines = allText.Split('\n');
            
            // group by any that start with '# '
            parsedChangeLog = new List<VersionData>();
            int startingIndex = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("# "))
                {
                    if (i > startingIndex)
                    {
                        List<string> entryLines = new List<string>();
                        for (int j = startingIndex + 1; j < i - 1; j++)
                        {
                            string line = lines[j];
                            if (entryLines.Count == 0 && line.Trim().Length == 0)
                            {
                                continue;
                            }

                            entryLines.Add(line);
                        }
                        
                        lines[startingIndex] = "v" + lines[startingIndex].Substring(1).Trim();
                        parsedChangeLog.Add(new VersionData(lines[startingIndex], entryLines.ToArray()));
                    }
                    startingIndex = i;
                }
            }
        }

        private void DrawChanges()
        {
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontSize = 18;
            
            
            for (int i = 0; i < parsedChangeLog.Count; i++)
            {
                VersionData data = parsedChangeLog[i];
                data.foldoutOpen = EditorGUILayout.Foldout(data.foldoutOpen, data.title, true, foldoutStyle);
                if (!data.foldoutOpen)
                {
                    continue;
                }
                
                EditorGUI.indentLevel++;
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    Draw(data.lines);
                }
                EditorGUI.indentLevel--;
                
                if (i != 0)
                {
                    GUILayout.Space(10);
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                    GUILayout.Space(10);
                }
            }
        }

        private void Draw(string[] lines)
        {
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
                    style = headerLabelStyle;
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
                    string color = Utils.IsDarkMode ? "#FFFB00" : "#7F7900";
                    string coloredSnippet = $"<color={color}>{codeSnippet}</color>";
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
                
                
                EditorGUILayout.TextField(line, style);
            }
        }

        private static void DrawLinks()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Documentation", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader/wiki");
                }
                
                if (GUILayout.Button(new GUIContent("Discord", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://discord.gg/R2UjXB6pQ8");
                }
                
                if (GUILayout.Button(new GUIContent("Github", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader");
                }
                
                if (GUILayout.Button(new GUIContent("Asset Store", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/build-uploader-306907");
                }
                
                if (GUILayout.Button(new GUIContent("Report Bug / Suggest Feature", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://github.com/JamesVeug/UnitySteamBuildUploader/issues");
                }
                
                if (GUILayout.Button(new GUIContent("Support Me", Utils.LinkIcon)))
                {
                    Application.OpenURL("https://buymeacoffee.com/jamesgamesnz");
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