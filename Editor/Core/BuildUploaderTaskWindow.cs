using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildUploaderTaskWindow : EditorWindow
    {
        [MenuItem("Window/Build Uploader/Open Upload Tasks Window", priority = -99)]
        public static void ShowWindow()
        {
            FocusTask(null);
        }

        public static void FocusTask(UploadTask uploadTask)
        {
            BuildUploaderTaskWindow window = GetWindow<BuildUploaderTaskWindow>();
            window.titleContent = new GUIContent("Upload Tasks", Utils.WindowIcon);
            window.Show();
            
            if (uploadTask != null)
            {
                window.ShowTask(uploadTask);
            }
        }

        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        
        private List<UploadTask> m_loadedTasks;
        private Vector2 m_scrollPosition;
        private Vector2 m_reportErrorScrollPosition;
        private string m_OpenTaskGUID = "";
        private Dictionary<AUploadTask_Step.StepType, (bool, Vector2)> m_OpenTaskSteps;
        private bool m_FollowLogs = true;

        public void Update()
        {
            bool anyRunning = UploadTask.AllTasks.Any(t => !t.IsComplete);
            if (anyRunning)
            {
                // Force repaint to update progress bars
                Repaint();
            }
        }


        private void Setup()
        {
            if (m_OpenTaskSteps != null)
            {
                return;
            }

            m_OpenTaskSteps = new Dictionary<AUploadTask_Step.StepType, (bool, Vector2)>();
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            
            m_subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            ReadAllTaskReports(false);
        }

        public void OnGUI()
        {
            Setup();
            
            // Header
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Upload Tasks", m_titleStyle);
                    GUILayout.FlexibleSpace();
                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Refresh"), false, ()=>ReadAllTaskReports(true));
                        
                        menu.AddSeparator("");
                        
                        menu.AddItem(new GUIContent("Delete All Complete Reports"), false, () =>
                        {
                            if (EditorUtility.DisplayDialog("Delete all Task Reports",
                                    "Are you sure you want to delete all task reports?\n\nThis will delete them from the cache folder and can NOT be undone!", "Delete All", "No"))
                            {
                                Directory.Delete(WindowUploadTab.UploadReportSaveDirectory, true);
                                m_loadedTasks = new List<UploadTask>();
                            }
                        });

                        menu.ShowAsContext();
                    }
                }

                // Column headers
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(120));
                    GUILayout.Label("Started", EditorStyles.boldLabel);
                    GUILayout.Label("Step", EditorStyles.boldLabel, GUILayout.Width(100));
                    GUILayout.Label("Progress", EditorStyles.boldLabel, GUILayout.Width(170));
                    GUILayout.Label("State", EditorStyles.boldLabel, GUILayout.Width(90));
                }

                EditorGUILayout.Space(2);

                var tasks = m_loadedTasks.Concat(UploadTask.AllTasks)
                    .Where(a=>a.Report != null)
                    .OrderBy(a=>a.Report.StartTime)
                    .ToArray();
                if (tasks.Length == 0)
                {
                    EditorGUILayout.HelpBox("No Task started this session. Use the Upload tab to begin uploading!", MessageType.Info);
                    return;
                }

                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (var i = 0; i < tasks.Length; i++)
                {
                    // Derive state and color
                    DrawTask(tasks[i]);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawTask(UploadTask t)
        {
            string stateText;
            Color stateColor;

            if (t.IsComplete)
            {
                if (t.IsSuccessful)
                {
                    stateText = "Success";
                    stateColor = new Color(0.22f, 0.7f, 0.3f); // green-ish
                }
                else
                {
                    stateText = "Failed";
                    stateColor = new Color(0.8f, 0.25f, 0.25f); // red-ish
                }
            }
            else
            {
                if (t.PercentComplete > 0f || t.CurrentSteps != null)
                {
                    stateText = "In Progress";
                    stateColor = new Color(0.95f, 0.65f, 0.1f); // amber
                }
                else
                {
                    stateText = "Idle";
                    stateColor = new Color(0.6f, 0.6f, 0.6f); // grey
                }
            }

            // Current step label
            string stepLabel = t.IsComplete ? "Done" : t.CurrentStep.ToString();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool isOpen = m_OpenTaskGUID == t.GUID;
                    Rect foldRect = GUILayoutUtility.GetRect(14, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(false));
                    bool newIsOpen = EditorGUI.Foldout(foldRect, isOpen, GUIContent.none, true);
                    if (newIsOpen != isOpen)
                    {
                        m_OpenTaskGUID = newIsOpen ? t.GUID : "";
                    }

                    // GUID
                    GUILayout.Label(t.UploadName, GUILayout.Width(100));

                    // Description (flex)
                    string duration = ""; 
                    if (t.IsComplete)
                    {
                        duration += "Took: " + t.Report.Duration.CalculateShortTime();
                    }
                    else
                    {
                        TimeSpan timeSoFar = DateTime.UtcNow - t.Report.StartTime;
                        duration = "Duration: " + timeSoFar.CalculateShortTime();
                    }
                        
                    GUILayout.Label(string.Format("{0} ({1})", t.Report.StartTime, duration));

                    // Step
                    GUILayout.Label(stepLabel, GUILayout.Width(100));

                    // Progress bar (170px area: bar + % text overlaid)
                    Rect r = GUILayoutUtility.GetRect(160, 16, GUILayout.Width(170), GUILayout.Height(16));
                    float pct = Mathf.Clamp01(t.PercentComplete);
                    EditorGUI.ProgressBar(r, pct, $"{Mathf.RoundToInt(pct * 100f)}%");

                    // State (colored)
                    var prev = GUI.color;
                    GUI.color = stateColor;
                    GUILayout.Label(stateText, EditorStyles.boldLabel, GUILayout.Width(90));
                    GUI.color = prev;
                }

                // Foldout details
                if (m_OpenTaskGUID == t.GUID)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        // Basics grid
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label("GUID", GUILayout.Width(120));
                            GUILayout.Label(t.GUID);
                        }
                        
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Description", GUILayout.Width(120));
                            GUILayout.Label(string.IsNullOrEmpty(t.UploadDescription) ? "<no description>" : t.UploadDescription);
                        }

                        if (t.Report == null)
                        {
                            EditorGUILayout.LabelField("Not started yet");
                            return;
                        }

                        // Failure reasons (via context/report) when failed
                        if (t.IsComplete && !t.IsSuccessful)
                        {
                            string failText = "";
                            foreach ((AUploadTask_Step.StepType Key, string FailReason) reason in t.Report.GetFailReasons())
                            {
                                if (string.IsNullOrEmpty(failText))
                                {
                                    failText += $"{reason.Key}: {reason.FailReason}";
                                }
                                else
                                {
                                    failText += $"\n{reason.Key}: {reason.FailReason}";
                                }
                            }

                            EditorGUILayout.Space(2);
                            EditorGUILayout.LabelField("Failure Details", EditorStyles.boldLabel);
                            if (!string.IsNullOrEmpty(failText))
                            {
                                m_reportErrorScrollPosition = EditorGUILayout.BeginScrollView(m_reportErrorScrollPosition);
                                EditorGUILayout.HelpBox(failText, MessageType.Error);
                                EditorGUILayout.EndScrollView();
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("No specific failure reasons provided.",
                                    MessageType.Error);
                            }
                        }

                        // Logs
                        if (t.Report.StepResults == null)
                        {
                            return;
                        }
                        
                        EditorGUILayout.Space(5);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Follow logs toggle
                            m_FollowLogs = GUILayout.Toggle(m_FollowLogs, "Follow Logs", EditorStyles.miniButton, GUILayout.Width(100));
                        }

                        // Show logs in a scrollable area
                        var steps = Enum.GetValues(typeof(AUploadTask_Step.StepType)).Cast<AUploadTask_Step.StepType>().ToArray();
                        if (m_FollowLogs)
                        {
                            Array.Reverse(steps); // Descending
                            
                            AUploadTask_Step.StepType stepToShow = AUploadTask_Step.StepType.GetSources;
                            foreach (AUploadTask_Step.StepType stepType in steps)
                            {
                                int logs = t.Report.CountStepLogs(stepType);
                                if (logs > 0)
                                {
                                    stepToShow = stepType;
                                    break;
                                }
                            }

                            Array.Reverse(steps); // Ascending
                            foreach (AUploadTask_Step.StepType stepType in steps)
                            {
                                if (!m_OpenTaskSteps.TryGetValue(stepType, out (bool, Vector2) pair))
                                {
                                    continue;
                                }
                                
                                Vector2 position = pair.Item2;
                                if (stepType == stepToShow)
                                {
                                    position.y = int.MaxValue;
                                    m_OpenTaskSteps[stepType] = (true, position);
                                }
                                else
                                {
                                    m_OpenTaskSteps[stepType] = (false, position);
                                }
                            }
                        }

                        foreach (AUploadTask_Step.StepType stepType in steps)
                        {
                            (bool foldout, Vector2 position) stepUI = (false, Vector2.zero);
                            if (m_OpenTaskSteps.TryGetValue(stepType, out (bool, Vector2) pair))
                            {
                                stepUI = pair;
                            }
                            
                            int logs = t.Report.CountStepLogs(stepType);

                            // Default to showing all steps
                            string label = logs > 0 ? $"{stepType} ({logs} logs)" : stepType.ToString();
                            stepUI.foldout = EditorGUILayout.Foldout(stepUI.foldout, label, true);
                            if (stepUI.foldout)
                            {
                                // Show logs for this step
                                StringBuilder sb = new StringBuilder();
                                stepUI.position = EditorGUILayout.BeginScrollView(stepUI.position, GUILayout.ExpandHeight(true), GUILayout.MinHeight(300));
                                t.Report.GetStepLogs(true, stepType, sb);
                                EditorGUILayout.TextArea(sb.ToString(), GUILayout.ExpandHeight(true), GUILayout.MinHeight(300));
                                EditorGUILayout.EndScrollView();
                            }
                            m_OpenTaskSteps[stepType] = stepUI;
                        }
                    }
                }
            }
        }

        public void ShowTask(UploadTask uploadTask)
        {
            if (uploadTask == null)
            {
                Debug.LogWarning("Cannot show null UploadTask.");
                return;
            }

            m_OpenTaskGUID = uploadTask.GUID;
            m_FollowLogs = true;
        }

        private DateTime m_lastReportRead = DateTime.MinValue;
        private float delayBetweenReportReads = 60f; // seconds
        private void ReadAllTaskReports(bool forceRefresh)
        {
            if (!forceRefresh && (DateTime.UtcNow - m_lastReportRead).TotalSeconds < delayBetweenReportReads)
            {
                return;
            }
            m_lastReportRead = DateTime.UtcNow;
            
            
            m_loadedTasks = new List<UploadTask>();
            if (!Directory.Exists(WindowUploadTab.UploadReportSaveDirectory))
            {
                return;
            }
            
            
            
            string[] filePaths = Directory.GetFiles(WindowUploadTab.UploadReportSaveDirectory, "*.txt", SearchOption.AllDirectories);
            foreach (string filePath in filePaths)
            {
                UploadTaskReport report = UploadTaskReport.FromFilePath(filePath);
                if (report == null)
                {
                    continue;
                }

                UploadTask task = new UploadTask();
                task.SetReport(report);
                m_loadedTasks.Add(task);
            }
        }
    }
}