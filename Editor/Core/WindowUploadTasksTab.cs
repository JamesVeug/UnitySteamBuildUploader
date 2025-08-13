using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class WindowUploadTasksTab : WindowTab
    {
        public override string TabName
        {
            get
            {
                int inProgress = UploadTask.AllTasks.Count(t => !t.IsComplete);
                if (inProgress != 0)
                {
                    return "Tasks (" + inProgress + " in progress)";
                }
                return "Tasks";
            }
        }

        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        
        private Vector2 m_reportErrorScrollPosition;
        private Vector2 m_reportLogsScrollPosition;
        private string m_OpenTaskGUID = "";
        private bool m_OpenTaskShowsLogs = true;
        private bool m_FollowLogs = true;

        public override void Initialize(BuildUploaderWindow uploaderWindow)
        {
            base.Initialize(uploaderWindow);
            
        }

        public override void Update()
        {
            base.Update();
            bool anyRunning = UploadTask.AllTasks.Any(t => !t.IsComplete);
            if (anyRunning)
            {
                // Force repaint to update progress bars
                UploaderWindow.Repaint();
            }
        }


        private void Setup()
        {
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
        }

        public override void OnGUI()
        {
            Setup();
            
            // Header
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Upload Tasks", m_titleStyle);
                
                // Column headers
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(120));
                    GUILayout.Label("Description", EditorStyles.boldLabel);
                    GUILayout.Label("Step", EditorStyles.boldLabel, GUILayout.Width(140));
                    GUILayout.Label("Progress", EditorStyles.boldLabel, GUILayout.Width(170));
                    GUILayout.Label("State", EditorStyles.boldLabel, GUILayout.Width(90));
                }

                EditorGUILayout.Space(2);

                var tasks = UploadTask.AllTasks;
                if (tasks == null || tasks.Count == 0)
                {
                    EditorGUILayout.HelpBox("No UploadTask instances found.", MessageType.Info);
                    return;
                }

                foreach (UploadTask t in tasks)
                {
                    // Derive state and color
                    DrawTask(t);
                }
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
                    Rect foldRect = GUILayoutUtility.GetRect(14, EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(false));
                    bool newIsOpen = EditorGUI.Foldout(foldRect, isOpen, GUIContent.none, true);
                    if (newIsOpen != isOpen)
                    {
                        m_OpenTaskGUID = newIsOpen ? t.GUID : "";
                    }

                    // GUID
                    GUILayout.Label(t.UploadName, GUILayout.Width(100));

                    // Description (flex)
                    GUILayout.Label(
                        string.IsNullOrEmpty(t.UploadDescription) ? "<no description>" : t.UploadDescription,
                        GUILayout.ExpandWidth(true));

                    // Step
                    GUILayout.Label(stepLabel, GUILayout.Width(140));

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
                    // EditorGUILayout.Space(2);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        // Basics grid
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("GUID", t.GUID);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (t.IsComplete)
                            {
                                EditorGUILayout.LabelField("Duration", t.Report.Duration.CalculateTime());
                            }
                            else
                            {
                                TimeSpan duration = DateTime.UtcNow - t.Report.StartTime;
                                EditorGUILayout.LabelField("Duration", duration.CalculateTime());
                            }
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
                                m_reportErrorScrollPosition = EditorGUILayout.BeginScrollView(m_reportErrorScrollPosition, GUILayout.Height(100));
                                EditorGUILayout.HelpBox(failText, MessageType.Error);
                                EditorGUILayout.EndScrollView();
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("No specific failure reasons provided.",
                                    MessageType.Error);
                            }
                        }

                        EditorGUILayout.Space(5);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            Rect foldRect = GUILayoutUtility.GetRect(14, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(false));
                            m_OpenTaskShowsLogs = EditorGUI.Foldout(foldRect, m_OpenTaskShowsLogs, "Show Report Logs", true);

                            EditorGUILayout.Space();

                            // Follow logs toggle
                            m_FollowLogs = GUILayout.Toggle(m_FollowLogs, "Follow Logs", EditorStyles.miniButton, GUILayout.Width(100));
                        }

                        if (m_OpenTaskShowsLogs)
                        {
                            // Logs
                            EditorGUILayout.Space(2);

                            // Show logs in a scrollable area
                            if (m_FollowLogs)
                            {
                                m_reportLogsScrollPosition = new Vector2(0, float.MaxValue);
                            }

                            m_reportLogsScrollPosition = EditorGUILayout.BeginScrollView(m_reportLogsScrollPosition, GUILayout.ExpandHeight(true));
                            EditorGUILayout.TextArea(t.Report.GetReport(true));
                            EditorGUILayout.EndScrollView();
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
    }
}