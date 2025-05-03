using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildUploaderReportWindow : EditorWindow
    {
        private BuildTaskReport report;

        private string txt;
        private Vector2 scroll;

        public static void ShowWindow(BuildTaskReport report, string taskReport=null)
        {
            var window = GetWindow<BuildUploaderReportWindow>("Build Uploader Report");
            
            Rect windowPosition = window.position;
            windowPosition.size = new Vector2(Screen.currentResolution.width * 0.5f, Screen.currentResolution.height * 0.5f);
            windowPosition.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = windowPosition;
            
            window.report = report;
            window.txt = taskReport ?? report.GetReport();
            window.ShowPopup();
        }

        private void OnGUI()
        {
            if (report == null)
            {
                EditorGUILayout.LabelField("No report available.");
                return;
            }

            if (GUILayout.Button("Save"))
            {
                string fileName = $"BuildReport_{report.StartTime:yyyy-MM-dd_HH-mm-ss}.txt";
                string path = EditorUtility.SaveFilePanel("Save Report", "", fileName, "txt");
                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.WriteAllText(path, txt);
                }
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(txt);
            EditorGUILayout.EndScrollView();
        }
    }
}