using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    public class SteamBuildWindowUnityCloudBuild
    {
        private UnityCloudBuild build;
        private bool downloading;
        private bool cancelling;

        public SteamBuildWindowUnityCloudBuild(UnityCloudBuild build)
        {
            this.build = build;
        }

        public void OnGUICollapsed()
        {
            DrawBuildName(GUILayout.Width(250));

            DrawStatus(GUILayout.Width(100));

            if (build.IsFinished)
            {
                DrawFinishedTime(GUILayout.Width(100));
            }
            else
            {
                DrawBuildStartTime(GUILayout.Width(100));
            }

            DrawLastAuthor(GUILayout.Width(100));
            DrawChanges(GUILayout.Width(100));
            DrawLastCommit();

            if (build.IsFinished)
            {
                if (build.IsSuccessful && build.HasArtifacts)
                {
                    using (new EditorGUI.DisabledScope(downloading || !build.IsFinished || !build.IsSuccessful))
                    {
                        if (GUILayout.Button("Download", GUILayout.Width(100)))
                        {
                            string folder = EditorUtility.OpenFolderPanel("Download Artifacts", ".", "");
                            DownloadArtifacts(folder);
                        }
                    }
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(cancelling))
                {
                    Color temp = GUI.color;
                    GUI.color = Color.red;
                    if (GUILayout.Button("CANCEL", GUILayout.Width(100)))
                    {
                        string message =
                            string.Format("Are you sure you want to cancel build {0}?\nThis can NOT be undone!",
                                build.build);
                        if (EditorUtility.DisplayDialog("Cancel Build", message, "Yes", "Cancel"))
                        {
                            CancelBuild();
                        }
                    }

                    GUI.color = temp;
                }
            }
        }

        private void DrawBuildName(params GUILayoutOption[] options)
        {
            DrawLabel("#" + build.build + " " + build.buildTargetName, options);
        }

        private void DrawLabel(string text, params GUILayoutOption[] options)
        {
            Color temp = GUI.color;

            GUI.color = GetBuildTextColor();
            GUILayout.Label(text, options);
            GUI.color = temp;
        }

        private void DrawLastCommit(params GUILayoutOption[] options)
        {
            List<UnityCloudBuild.ArtifactChange> changes = build.GitChangeLogs;

            string changeText = "";
            if (changes.Count == 0)
            {
                changeText = "";
            }
            else
            {
                changeText = changes[0].message;
            }


            DrawLabel(changeText, options);
        }

        private void DrawLastAuthor(params GUILayoutOption[] options)
        {
            List<UnityCloudBuild.ArtifactChange> changes = build.GitChangeLogs;

            string changeText = "";
            if (changes.Count == 0)
            {
                changeText = "";
            }
            else
            {
                changeText = changes[0].author.fullName;
            }

            DrawLabel(changeText, options);
        }

        private void DrawChanges(params GUILayoutOption[] options)
        {
            List<UnityCloudBuild.ArtifactChange> changes = build.GitChangeLogs;

            string changeText = "";
            if (changes.Count == 0)
            {
                changeText = "";
            }
            else
            {
                changeText = string.Format("{0} changes", changes.Count);
            }

            DrawLabel(changeText, options);
        }

        private void DrawFinishedTime(params GUILayoutOption[] options)
        {
            string timeString = "";
            if (build.IsFinished && build.finished != null)
            {
                DateTime buildCreatedDateTime = build.FinishedDateTime;
                DateTime utcNow = DateTime.UtcNow;

                TimeSpan timeSpan = utcNow - buildCreatedDateTime;

                if (timeSpan.TotalDays >= 365)
                {
                    timeString = "1+ years ago";
                }
                else if (timeSpan.TotalDays >= 7)
                {
                    int weeks = (int)(timeSpan.TotalDays / 7);
                    timeString = weeks + " weeks ago";
                }
                else if (timeSpan.TotalDays > 1)
                {
                    timeString = (int)timeSpan.TotalDays + " days ago";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    timeString = (int)timeSpan.TotalHours + " hours ago";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    timeString = (int)timeSpan.TotalMinutes + " minutes ago";
                }
                else
                {
                    timeString = "< 1 minute ago";
                }
            }

            DrawLabel(timeString, options);
        }

        private void DrawBuildStartTime(params GUILayoutOption[] options)
        {
            string timeString = "";
            if (build.buildStartTime != null)
            {
                DateTime buildCreatedDateTime = build.BuildStartDateTime;
                DateTime utcNow = DateTime.UtcNow;

                TimeSpan timeSpan = utcNow - buildCreatedDateTime;

                if (timeSpan.TotalDays >= 365)
                {
                    timeString = "1+ years";
                }
                else if (timeSpan.TotalDays >= 7)
                {
                    int weeks = (int)(timeSpan.TotalDays / 7);
                    timeString = weeks + " weeks";
                }
                else if (timeSpan.TotalDays > 1)
                {
                    timeString = (int)timeSpan.TotalDays + " days";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    timeString = (int)timeSpan.TotalHours + " hours";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    timeString = (int)timeSpan.TotalMinutes + " minutes";
                }
                else
                {
                    timeString = "< 1 minute";
                }
            }

            DrawLabel(timeString, options);
        }

        private void DrawStatus(params GUILayoutOption[] options)
        {
            Color oldColor = GUI.color;
            string buildStatus = build.buildStatus;
            if (buildStatus == "success")
                GUI.color = Color.green;
            else if (buildStatus == "failure")
                GUI.color = Color.red;
            else if (buildStatus == "started")
                GUI.color = Color.cyan;
            else if (buildStatus == "canceled")
                GUI.color = Color.gray;
            else
                GUI.color = Color.yellow;

            GUILayout.Label(buildStatus, options);
            GUI.color = oldColor;
        }

        public void DownloadArtifacts(string directory)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadArtifactsCoroutine(directory));
        }

        private IEnumerator DownloadArtifactsCoroutine(string directory)
        {
            downloading = true;
            yield return UnityCloudAPI.DownloadBuildArtifacts(build, directory);
            downloading = false;
        }

        public void CancelBuild()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CancelBuildCoroutine());
        }

        private IEnumerator CancelBuildCoroutine()
        {
            cancelling = true;

            UnityWebRequest request = UnityCloudAPI.CancelBuild(build.buildtargetid, build.build);
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                string downloadHandlerText = request.downloadHandler?.text;
                string message = string.Format("Could not cancel build {0} with UnityCloud:\nError: {1}", build.build,
                    downloadHandlerText);
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Cancelled Build Failed", message, "OK");
                yield break;
            }

            Debug.Log("Build Cancelled");
            if (!UnityCloudAPI.IsSyncing)
            {
                UnityCloudAPI.SyncBuilds();
            }

            EditorUtility.DisplayDialog("Build Cancelled Successful", "Successfully cancelled build: #" + build.build,
                "OK");
            cancelling = false;
        }

        private Color GetBuildTextColor()
        {
            if (build.buildStatus == "canceled")
            {
                return Color.gray;
            }
            else if (build.buildStatus == "started")
            {
                return Color.cyan;
            }
            else if (build.buildStatus == "queued")
            {
                return Color.yellow;
            }

            return Color.white;
        }
    }
}