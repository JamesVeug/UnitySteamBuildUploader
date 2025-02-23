using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    internal class UnityCloudSource : ABuildSource
    {
        private string sourceFilePath;
        private UnityCloudTarget sourceTarget;
        private UnityCloudBuild sourceBuild;

        private Vector2 buildScrollPosition;

        private BuildUploaderWindow _uploaderWindow;
        private string unzipDirectory;
        private string fullFilePath;

        public UnityCloudSource(BuildUploaderWindow buildUploaderWindow)
        {
            sourceFilePath = null;
            _uploaderWindow = buildUploaderWindow;
        }

        public override void OnGUIExpanded(ref bool isDirty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Target:", GUILayout.Width(120));
                isDirty |= UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Build:", GUILayout.Width(120));
                using (new EditorGUI.DisabledScope(UnityCloudAPI.IsSyncing))
                {
                    if (GUILayout.Button("Refresh", GUILayout.Width(75)))
                    {
                        UnityCloudAPI.SyncBuilds();
                        //UnityCloudAPIEditorUtil.TargetPopup.Refresh();
                        _uploaderWindow.Repaint();
                    }
                }

                buildScrollPosition = EditorGUILayout.BeginScrollView(buildScrollPosition, GUILayout.MaxHeight(100));
                using (new EditorGUILayout.VerticalScope())
                {
                    List<UnityCloudBuild> builds = UnityCloudAPI.GetBuildsForTarget(sourceTarget);
                    if (builds != null)
                    {
                        for (int i = 0; i < builds.Count; i++)
                        {
                            UnityCloudBuild build = builds[i];
                            bool isSelected = sourceBuild != null &&
                                              sourceBuild.CreateBuildName() == build.CreateBuildName();
                            using (new EditorGUI.DisabledScope(isSelected || UnityCloudAPI.IsSyncing))
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(build.CreateBuildName()))
                                    {
                                        sourceBuild = build;
                                        isDirty = true;
                                    }
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        public override void OnGUICollapsed(ref bool isDirty)
        {
            if (UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget))
            {
                isDirty = true;
            }

            if (UnityCloudAPIEditorUtil.BuildPopup.DrawPopup(sourceTarget, ref sourceBuild))
            {
                isDirty = true;
            }
        }

        public override async Task<bool> GetSource()
        {
            m_getSourceInProgress = true;
            m_downloadProgress = 0.0f;

            // Preparing
            m_progressDescription = "Preparing...";
            string buildName = sourceBuild.platform + "-" + sourceBuild.buildtargetid + "-" + sourceBuild.build;
            string directoryPath = Application.persistentDataPath + "/UnityCloudBuilds";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            fullFilePath = directoryPath + "/" + buildName + ".zip";

            // Only download if we don't have it
            if (!File.Exists(fullFilePath))
            {
                string downloadUrl = sourceBuild.GetGameArtifactDownloadUrl();
                if (downloadUrl == null)
                {
                    downloadUrl = sourceBuild.GetAddressableArtifactDownloadUrl();
                    if (downloadUrl == null)
                    {
                        Debug.Log("Could not download UnityCloudBuild. No artifacts in build!");
                        return false;
                    }
                }

                Debug.Log("Downloading from: " + downloadUrl);

                m_progressDescription = "Fetching...";
                UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
                UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();

                // Wait for it to be downloaded?
                while (!webRequest.isDone)
                {
                    await Task.Delay(10);
                    m_downloadProgress = request.downloadProgress;
                    m_progressDescription = "Downloading from UnityCloud...";
                }

                // Save
                m_progressDescription = "Saving locally...";
                await File.WriteAllBytesAsync(fullFilePath, request.downloadHandler.data);
            }
            else
            {
                Debug.Log("Skipping downloading form UnityCloud since it already exists: " + fullFilePath);
            }

            m_progressDescription = "Done!";
            Debug.Log("Retrieved UnityCloud Build: " + fullFilePath);

            // Record where the game is saved to
            sourceFilePath = fullFilePath;
            m_downloadProgress = 1.0f;
            return true;
        }

        public override string SourceFilePath()
        {
            return sourceFilePath;
        }

        public override float DownloadProgress()
        {
            return m_downloadProgress;
        }

        public override string ProgressTitle()
        {
            return "Downloading from UnityCloud";
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup(out string reason)
        {
            if (sourceTarget == null)
            {
                reason = "No target selected";
                return false;
            }
            
            if (sourceBuild == null)
            {
                reason = "No build selected";
                return false;
            }

            reason = "";
            return true;
        }

        public override string GetBuildDescription()
        {
            return sourceBuild.CreateBuildName();
        }

        public override void AssignLatestBuildTarget()
        {
            base.AssignLatestBuildTarget();

            List<UnityCloudBuild> unityCloudBuilds = UnityCloudAPI.GetBuildsForTarget(sourceTarget);
            UnityCloudBuild lastTarget = unityCloudBuilds[unityCloudBuilds.Count - 1];
            sourceBuild = lastTarget;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["sourceTarget"] = sourceTarget?.name
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            string sourceTargetName = (string)data["sourceTarget"];

            List<UnityCloudTarget> buildTargets = UnityCloudAPI.CloudBuildTargets;
            if (buildTargets != null)
            {
                for (int i = 0; i < buildTargets.Count; i++)
                {
                    if (buildTargets[i].name == sourceTargetName)
                    {
                        sourceTarget = buildTargets[i];
                    }
                }
            }
        }
    }
}