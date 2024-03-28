using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Wireframe
{
    public class SteamBuildUnityCloudSource : ASteamBuildSource
    {
        private string sourceFilePath;
        private UnityCloudTarget sourceTarget;
        private UnityCloudBuild sourceBuild;

        private Vector2 buildScrollPosition;

        private SteamBuildWindow window;
        private string unzipDirectory;
        private string fullFilePath;

        public SteamBuildUnityCloudSource(SteamBuildWindow steamBuildWindow)
        {
            sourceFilePath = null;
            window = steamBuildWindow;
        }

        public override void OnGUIExpanded()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Target:", GUILayout.Width(120));
                UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget);
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
                        window.Repaint();
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
                                    }
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        public override void OnGUICollapsed()
        {
            UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref sourceTarget);
            UnityCloudAPIEditorUtil.BuildPopup.DrawPopup(sourceTarget, ref sourceBuild);
        }

        public override IEnumerator GetSource()
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
                        yield break;
                    }
                }

                Debug.Log("Downloading from: " + downloadUrl);

                m_progressDescription = "Fetching...";
                UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
                EditorCoroutineUtility.StartCoroutineOwnerless(WaitForRequestToFinish(request));

                // Wait for it to be downloaded?
                while (request.isDone == false)
                {
                    yield return null;
                    m_downloadProgress = request.downloadProgress / 2.0f; // 50% is downloading, other 50% is unpacking
                    m_progressDescription = "Downloading from UnityCloud...";
                }

                // Save
                m_progressDescription = "Saving locally...";
                File.WriteAllBytes(fullFilePath, request.downloadHandler.data);
            }
            else
            {
                Debug.Log("Skipping downloading form UnityCloud since it already exists: " + fullFilePath);
            }



            // Decide where we want to download to
            // unzipDirectory = directoryPath + "/" + buildName;
            // if (!Directory.Exists(unzipDirectory))
            // {
            //     m_progressDescription = "Unzipping...";
            //
            //     Debug.Log("Unzipping to '" + unzipDirectory + "'");
            //     byte[] fileBytes = null;
            //     try
            //     {
            //         fileBytes = File.ReadAllBytes(fullFilePath);
            //     }
            //     catch (Exception e)
            //     {
            //         Debug.LogError("Error trying to get file byes: " + e.ToString());
            //     }
            //
            //     UnZipResult result = new UnZipResult();
            //     IEnumerator extractZipFile = ExtractZipFile(fileBytes, unzipDirectory, 256 * 1024, result);
            //     while (extractZipFile.MoveNext())
            //     {
            //         m_downloadProgress = 0.5f + result.unzipPercentage / 2.0f;
            //         yield return null;
            //     }
            //
            //     Debug.Log("Unzipped");
            // }
            // else
            // {
            //     Debug.Log("Skipping unzipping as directory already exists: " + unzipDirectory);
            // }

            m_progressDescription = "Done!";
            Debug.Log("Retrieved UnityCloud Build: " + fullFilePath);

            // Record where the game is saved to
            sourceFilePath = fullFilePath;
            m_downloadProgress = 1.0f;
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

        public override bool IsSetup()
        {
            return sourceTarget != null && sourceBuild != null;
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

        private IEnumerator WaitForRequestToFinish(UnityWebRequest request)
        {
            yield return request.SendWebRequest();
        }
    }
}