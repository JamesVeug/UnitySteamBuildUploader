using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<bool> m_viewTextDropdown = new List<bool>();

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
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    DrawTaskSteps(task);

                    if(m_viewTextDropdown.Count <= i)
                    {
                        m_viewTextDropdown.Add(false);
                    }
                    m_viewTextDropdown[i] = EditorGUILayout.Foldout(m_viewTextDropdown[i], "View Task Report");
                    if (m_viewTextDropdown[i])
                    {
                        GUILayout.TextArea(task.Report.GetReport());
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawTaskSteps(UploadTask task)
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
            EditorGUILayout.Space(10);
            
            float width = UploaderWindow.position.width / Enum.GetValues(typeof(AUploadTask_Step.StepType)).Length - 50;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };

                foreach (AUploadTask_Step.StepType stepType in Enum.GetValues(typeof(AUploadTask_Step.StepType)))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label(stepType.ToString(), headerStyle, GUILayout.Width(width));
                        float progress = task.Report.GetProgress(stepType, AUploadTask_Step.StepProcess.Intra);
                        GUILayout.Label($"{progress:P0}", GUILayout.Width(width));
                    }
                }
            }
            
            // Data
            // foreach (AUploadTask_Step.StepType stepType in Enum.GetValues(typeof(AUploadTask_Step.StepType)))
            // {
            //     var stepData = results.GetValueOrDefault(stepType) ?? new();
            //
            //     using (new EditorGUILayout.HorizontalScope())
            //     {
            //         GUILayout.Label($"- {stepType}", GUILayout.Width(200));
            //         foreach (AUploadTask_Step.StepProcess process in Enum.GetValues(typeof(AUploadTask_Step.StepProcess)))
            //         {
            //             var processData = stepData.GetValueOrDefault(process) ?? new();
            //             int maxProcesses = Mathf.Max(1, results.Max(a => a.Value.GetValueOrDefault(process)?.Count ?? 0));
            //
            //             foreach (var stepResult in processData)
            //             {
            //                 GUILayout.Label($"{stepResult.PercentComplete:P0}", GUILayout.Width(50));
            //                 maxProcesses--;
            //                 // foreach (var log in stepResult.Logs)
            //                 // {
            //                 //     GUILayout.Label($"[{log.Type}] {log.Message}");
            //                 // }
            //             }
            //
            //             for (int i = 0; i < maxProcesses; i++)
            //             {
            //                 GUILayout.Label($"", GUILayout.Width(50));
            //             }
            //             
            //         }
            //     }
            // }

            // Headers
            // var results = task.Report.StepResults;
            // using (new EditorGUILayout.HorizontalScope())
            // {
            //     GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            //     {
            //         fontSize = 14,
            //         fontStyle = FontStyle.Bold,
            //         alignment = TextAnchor.MiddleCenter
            //     };
            //     GUILayout.Label($"Steps", headerStyle, GUILayout.Width(200));
            //     foreach (AUploadTask_Step.StepProcess process in Enum.GetValues(typeof(AUploadTask_Step.StepProcess)))
            //     {
            //         int maxProcesses = Mathf.Max(1, results.Max(a => a.Value.GetValueOrDefault(process)?.Count ?? 0));
            //         for (int i = 0; i < maxProcesses; i++)
            //         {
            //             GUILayout.Label($"{process}", headerStyle, GUILayout.Width(50));
            //         }
            //     }
            // }
            //
            // // Data
            // foreach (AUploadTask_Step.StepType stepType in Enum.GetValues(typeof(AUploadTask_Step.StepType)))
            // {
            //     var stepData = results.GetValueOrDefault(stepType) ?? new();
            //
            //     using (new EditorGUILayout.HorizontalScope())
            //     {
            //         GUILayout.Label($"- {stepType}", GUILayout.Width(200));
            //         foreach (AUploadTask_Step.StepProcess process in Enum.GetValues(typeof(AUploadTask_Step.StepProcess)))
            //         {
            //             var processData = stepData.GetValueOrDefault(process) ?? new();
            //             int maxProcesses = Mathf.Max(1, results.Max(a => a.Value.GetValueOrDefault(process)?.Count ?? 0));
            //
            //             foreach (var stepResult in processData)
            //             {
            //                 GUILayout.Label($"{stepResult.PercentComplete:P0}", GUILayout.Width(50));
            //                 maxProcesses--;
            //                 // foreach (var log in stepResult.Logs)
            //                 // {
            //                 //     GUILayout.Label($"[{log.Type}] {log.Message}");
            //                 // }
            //             }
            //
            //             for (int i = 0; i < maxProcesses; i++)
            //             {
            //                 GUILayout.Label($"", GUILayout.Width(50));
            //             }
            //             
            //         }
            //     }
            // }
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
                float width = UploaderWindow.position.width / 3 - 100;
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label($"Sources", GUILayout.Width(width));
                    GUILayout.Label($"Modifiers", GUILayout.Width(width));
                    GUILayout.Label($"Destinations", GUILayout.Width(width));
                }

                foreach (UploadConfig config in task.UploadConfigs)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Color color = GUI.color;
                        GUI.color = Color.red;
                        GUI.color = color;
                        foreach (UploadConfig.SourceData source in config.Sources)
                        {
                            if (!source.Enabled)
                            {
                                continue;
                            }

                            using (new EditorGUI.DisabledScope(true))
                            {
                                if (UIHelpers.SourcesPopup.DrawPopup(ref source.SourceType, GUILayout.Width(100)))
                                {
                                    Utils.CreateInstance(source.SourceType?.Type, out source.Source);
                                }

                                source.Source.OnGUICollapsed(ref dirty, width, task.Context);
                            }
                        }

                        foreach (UploadConfig.ModifierData modifier in config.Modifiers)
                        {
                            if (!modifier.Enabled)
                            {
                                continue;
                            }

                            using (new EditorGUI.DisabledScope(true))
                            {
                                if (UIHelpers.ModifiersPopup.DrawPopup(ref modifier.ModifierType, GUILayout.Width(100)))
                                {
                                    Utils.CreateInstance(modifier.ModifierType?.Type, out modifier.Modifier);
                                }
                            }
                        }

                        // foreach (UploadConfig.DestinationData destination in config.Destinations)
                        // {
                        //     if (!destination.Enabled)
                        //     {
                        //         continue;
                        //     }
                        //
                        //     using (new EditorGUI.DisabledScope(true))
                        //     {
                        //         if (UIHelpers.DestinationsPopup.DrawPopup(ref destination.DestinationType, GUILayout.Width(100)))
                        //         {
                        //             Utils.CreateInstance(destination.DestinationType?.Type, out destination.Destination);
                        //         }
                        //         
                        //         destination.Destination.OnGUICollapsed(ref dirty, width, task.Context);
                        //     }
                        // }
                    }
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