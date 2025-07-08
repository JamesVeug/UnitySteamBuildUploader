using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildUploaderReportWindow : EditorWindow
    {
        private BuildTaskReport report;

        private string path;
        
        private string txt;
        private Vector2 scroll;

        [MenuItem("Window/Build Uploader/Build Reports", false, 0)]
        private static void ShowWindow()
        {
            ShowWindow(null, null);
        }

        public static void ShowWindow(BuildTaskReport report, string taskReport=null)
        {
            var window = GetWindow<BuildUploaderReportWindow>("Build Uploader Report");
            
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(Screen.currentResolution.width * 0.5f, Screen.currentResolution.height * 0.5f);
            windowPosition.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition;

            window.path = null;
            window.report = report;
            window.txt = taskReport != null ? taskReport : report != null ? report.GetReport() : "";
            window.ShowPopup();
        }

        private void OnGUI()
        {
            // UI to select a report to from your directory
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Path", GUILayout.Width(50));
                path = EditorGUILayout.TextField(path);
                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    string selectedFile = EditorUtility.OpenFilePanel("Select BuildReport", Preferences.CacheFolderPath, "txt");
                    if (!string.IsNullOrEmpty(selectedFile))
                    {
                        LoadFileAtPath(selectedFile);
                    }
                }
            }


            if (string.IsNullOrEmpty(txt))
            {
                EditorGUILayout.LabelField("No report available.");
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(txt);
            EditorGUILayout.EndScrollView();

            if (report != null)
            {
                if (GUILayout.Button("Save"))
                {
                    string fileName = $"BuildReport_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
                    string path = EditorUtility.SaveFilePanel("Save Report", "", fileName, "txt");
                    if (!string.IsNullOrEmpty(path))
                    {
                        System.IO.File.WriteAllText(path, txt);
                    }
                }
            }
        }

        private void LoadFileAtPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                Debug.LogError("File does not exist at the specified path.");
                return;
            }

            path = filePath;
            txt = System.IO.File.ReadAllText(path);
        }
    }
}