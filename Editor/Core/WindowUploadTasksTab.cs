using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class WindowUploadTasksTab : WindowTab
    {
        public override string TabName => "Upload Tasks";
        
        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        private Vector2 m_scrollPosition;

        public override void Initialize(BuildUploaderWindow uploaderWindow)
        {
            base.Initialize(uploaderWindow);
            
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

            GUILayout.Label("Upload Tasks", m_titleStyle);
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            for (var i = UploadTask.AllTasks.Count - 1; i >= 0; i--)
            {
                var task = UploadTask.AllTasks[i];
                DrawTask(task);
            }

            GUILayout.EndScrollView();
        }

        private void DrawTask(UploadTask task)
        {
            // Show:
            // - Task GUID
            // - In progress or completed?
            //      - % completion
            //      - successful or failed
            // - Logs in realtime
            // - Cancel button if in progress
            // - Clear all complete tasks button
            // - Retry button for complete tasks?

            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label($"Task GUID: {task.GUID}", m_subTitleStyle);

                if (task.IsComplete)
                {
                    GUILayout.Label($"Status: Completed {CalculateTimeAgo(task.Report.EndTime)} ago");
                    GUILayout.Label($"Successful: {task.IsSuccessful} ({task.Report.Duration.Seconds} seconds)");
                    if (!task.IsSuccessful)
                    {
                        string failReasons = "";
                        foreach ((AUploadTask_Step.StepType Key, string FailReason) valueTuple in task.Report
                                     .GetFailReasons())
                        {
                            if (string.IsNullOrEmpty(failReasons))
                            {
                                failReasons += $"{valueTuple.Key}: {valueTuple.FailReason}";
                            }
                            else
                            {
                                failReasons += $"\n{valueTuple.Key}: {valueTuple.FailReason}";
                            }
                        }

                        GUILayout.Label($"Failed Reasons: {failReasons}");
                    }
                }
                else
                {
                    GUILayout.Label($"Status: In Progress {task.PercentComplete:P0}");
                    GUILayout.Label($"Step: {task.CurrentStep}");
                    using (new EditorGUI.DisabledScope(true))
                    {
                        if (GUILayout.Button("Cancel"))
                        {
                            // task.Cancel();
                        }
                    }
                }

                bool dirty = false;
                foreach (UploadConfig config in task.UploadConfigs)
                {
                    GUILayout.Label($"Sources");
                    using (new EditorGUILayout.HorizontalScope("box"))
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            foreach (UploadConfig.SourceData source in config.Sources)
                            {
                                if (!source.Enabled)
                                {
                                    continue;
                                }

                                source.Source.OnGUICollapsed(ref dirty, UploaderWindow.position.width, task.Context);
                            }
                        }

                        GUILayout.Label($"Modifiers");
                        foreach (UploadConfig.ModifierData modifier in config.Modifiers)
                        {
                            if (!modifier.Enabled)
                            {
                                continue;
                            }

                            GUILayout.Label($"{modifier.ModifierType.DisplayName}");
                        }

                        GUILayout.Label($"Destinations");
                        foreach (UploadConfig.DestinationData destination in config.Destinations)
                        {
                            if (!destination.Enabled)
                            {
                                continue;
                            }

                            GUILayout.Label($"{destination.DestinationType.DisplayName}");
                        }
                    }
                }

                if (GUILayout.Button("View Report"))
                {
                    UploadTaskReportWindow.ShowWindow(task.Report);
                }
            }
        }

        private string CalculateTimeAgo(DateTime endTime)
        {
            var timeSpan = DateTime.UtcNow - endTime;
            if (timeSpan.TotalSeconds < 60)
            {
                return $"< 1 minute";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                return $"{timeSpan.Minutes} minutes";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"{timeSpan.Hours} hours";
            }
            else
            {
                return $"{timeSpan.Days} days";
            }
        }
    }
}