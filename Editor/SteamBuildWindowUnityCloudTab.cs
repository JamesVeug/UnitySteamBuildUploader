using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    public class SteamBuildWindowUnityCloudTab : SteamBuildWindowTab
    {
        private const int AutoRefreshTime = 60;

        private UnityCloudTarget currentTarget;

        private GUIStyle m_titleStyle;
        private GUIStyle m_targetFoldoutStyle;
        private Vector2 m_scrollPosition;

        private Dictionary<string, SteamBuildWindowUnityCloudBuild> m_cloudBuildToUIList;
        private Dictionary<string, bool> m_targetExpanded;

        private int m_cachedSyncs;

        private void Setup()
        {
            if (m_titleStyle == null)
            {
                m_titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }


            if (m_targetFoldoutStyle == null)
            {
                m_targetFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontSize = 17,
                };
            }

            if (m_cloudBuildToUIList == null)
            {
                m_cloudBuildToUIList = new Dictionary<string, SteamBuildWindowUnityCloudBuild>();
            }

            if (m_targetExpanded == null)
            {
                m_targetExpanded = new Dictionary<string, bool>();
            }
        }

        public override void Update()
        {
            base.Update();

            if (m_cachedSyncs != UnityCloudAPI.TotalSyncs)
            {
                m_cloudBuildToUIList = null;
                m_targetExpanded = null;

                window.Repaint();
            }

            double timeSinceLastRefresh = (DateTime.UtcNow - UnityCloudAPI.LastSyncDateTime).TotalSeconds;
            if (timeSinceLastRefresh > AutoRefreshTime || UnityCloudAPI.TotalSyncs == 0) // 5 minutes
            {
                if (window.CurrentTab == SteamBuildWindow.Tabs.UnityCloud || UnityCloudAPI.CloudBuildTargets == null)
                {
                    if (!UnityCloudAPI.IsSyncing)
                    {
                        UnityCloudAPI.SyncBuilds();
                    }
                }
            }
        }

        public override void OnGUI()
        {
            Setup();

            DrawSettings();

            EditorGUILayout.Space(20);

            DrawConfigs();

            EditorGUILayout.Space(20);

            DrawBuilds();
        }

        private void DrawBuilds()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Builds", m_titleStyle);

                m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

                List<(UnityCloudTarget, List<UnityCloudBuild>)> builds =
                    new List<(UnityCloudTarget, List<UnityCloudBuild>)>();
                var currentBuilds = UnityCloudAPI.CurrentBuilds;
                if (currentBuilds != null)
                {
                    builds.AddRange(currentBuilds); // Sometimes this is null on first boot
                }

                builds.Sort(SortBuilds);

                bool forceRefresh = builds == null && !UnityCloudAPI.IsSyncing;
                using (new EditorGUI.DisabledScope(UnityCloudAPI.IsSyncing))
                {
                    string text = "Refresh All Builds";
                    if (!UnityCloudAPI.IsSyncing)
                    {
                        double timeSinceLastRefresh = (DateTime.UtcNow - UnityCloudAPI.LastSyncDateTime).TotalSeconds;
                        int timeLeft = (int)(AutoRefreshTime - timeSinceLastRefresh);
                        text += string.Format("({0})", timeLeft);
                    }

                    if (GUILayout.Button(text) || forceRefresh)
                    {
                        UnityCloudAPI.SyncBuilds();
                    }
                }

                if (builds != null)
                {
                    // Builds by Target
                    for (int i = 0; i < builds.Count; i++)
                    {
                        (UnityCloudTarget, List<UnityCloudBuild>) buildTargetData = builds[i];
                        UnityCloudTarget buildTarget = buildTargetData.Item1;

                        using (new EditorGUI.DisabledScope(!buildTarget.enabled))
                        {
                            if (!m_targetExpanded.TryGetValue(buildTarget.buildtargetid, out var expanded))
                            {
                                expanded = false;
                                m_targetExpanded[buildTarget.buildtargetid] = expanded;
                            }

                            using (new GUILayout.VerticalScope("box"))
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    string buildName = buildTarget.name;
                                    if (!buildTarget.enabled)
                                    {
                                        buildName += " (Disabled)";
                                    }

                                    m_targetExpanded[buildTarget.buildtargetid] =
                                        EditorGUILayout.Foldout(expanded, buildName, m_targetFoldoutStyle);
                                    if (GUILayout.Button("Start Build", GUILayout.Width(100)))
                                    {
                                        StartTargetBuild(buildTarget.buildtargetid);
                                    }
                                }

                                List<UnityCloudBuild> unityCloudBuilds = buildTargetData.Item2;

                                GUILayout.Space(10);
                                for (int j = 0; j < unityCloudBuilds.Count; j++)
                                {
                                    UnityCloudBuild cloudBuild = unityCloudBuilds[j];
                                    if (!m_cloudBuildToUIList.TryGetValue(cloudBuild.CreateBuildName(), out var b))
                                    {
                                        b = new SteamBuildWindowUnityCloudBuild(cloudBuild);
                                        m_cloudBuildToUIList[cloudBuild.CreateBuildName()] = b;
                                    }

                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        b.OnGUICollapsed();
                                    }

                                    if (j >= 2 && !expanded)
                                    {
                                        // Only show 3 if we are collapsed
                                        break;
                                    }
                                }

                            }
                        }

                        GUILayout.Space(20);
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        private int SortBuilds((UnityCloudTarget, List<UnityCloudBuild>) x, (UnityCloudTarget, List<UnityCloudBuild>) y)
        {
            UnityCloudTarget aTarget = UnityCloudAPI.GetTargetForBuild(x.Item2[0]);
            UnityCloudTarget bTarget = UnityCloudAPI.GetTargetForBuild(y.Item2[0]);

            if (aTarget.enabled != bTarget.enabled)
            {
                return bTarget.enabled.CompareTo(aTarget.enabled);
            }

            if (aTarget.platform != bTarget.platform)
            {
                return String.Compare(bTarget.platform, aTarget.platform, StringComparison.Ordinal);
            }

            return aTarget.buildtargetid.Length - bTarget.buildtargetid.Length;
        }

        private void StartTargetBuild(string buildTargetID)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(StartTargetBuildCoroutine(buildTargetID));
        }

        private IEnumerator StartTargetBuildCoroutine(string buildTargetID)
        {
            UnityWebRequest request = UnityCloudAPI.StartBuild(buildTargetID);
            yield return request.SendWebRequest();

            string downloadHandlerText = request.downloadHandler.text;
            if (request.isHttpError || request.isNetworkError)
            {
                string message = string.Format("Could not start build target '{0}' with UnityCloud:\nError: {1}",
                    buildTargetID, downloadHandlerText);
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Start new Build Failed", message, "OK");
                yield break;
            }

            Debug.Log("Started build successfully: " + downloadHandlerText);
            if (!UnityCloudAPI.IsSyncing)
            {
                UnityCloudAPI.SyncBuilds();
            }

            EditorUtility.DisplayDialog("Target Build Started Successful",
                "Successfully started a build for target: " + buildTargetID, "OK");
        }

        private void DrawConfigs()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Configs", m_titleStyle);

                // Draw Dropdown list
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Target:", GUILayout.Width(100));
                    UnityCloudAPIEditorUtil.TargetPopup.DrawPopup(ref currentTarget);
                }

                if (currentTarget != null)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        // Draw target
                        using (new GUILayout.VerticalScope())
                        {
                            DrawBuildTarget();
                        }
                    }
                }
            }
        }

        private void DrawSettings()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Settings", m_titleStyle);
                using (new GUILayout.HorizontalScope())
                {
                    UnityCloud.Instance.Organization = PasswordField.Draw("Organization:", 100,UnityCloud.Instance.Organization);
                }

                using (new GUILayout.HorizontalScope())
                {
                    UnityCloud.Instance.Project = PasswordField.Draw("Project:", 100,UnityCloud.Instance.Project);
                }

                using (new GUILayout.HorizontalScope())
                {
                    UnityCloud.Instance.Secret = PasswordField.Draw("Secret:", 100,UnityCloud.Instance.Secret);
                }
            }
        }

        private void DrawBuildTarget()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("ID:", GUILayout.Width(150));
                string newTargetID = EditorGUILayout.TextField(currentTarget.buildtargetid);
                if (newTargetID != currentTarget.name)
                {
                    currentTarget.buildtargetid = newTargetID;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Name:", GUILayout.Width(150));
                string newTargetName = EditorGUILayout.TextField(currentTarget.name);
                if (newTargetName != currentTarget.name)
                {
                    currentTarget.name = newTargetName;
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Platform:", GUILayout.Width(150));
                Platform oldPlatform = currentTarget.platform.ToPlatformEnum();
                Platform newPlatform = (Platform)EditorGUILayout.EnumPopup(oldPlatform);
                if (oldPlatform != newPlatform)
                {
                    currentTarget.platform = newPlatform.ToPlatformString();
                    window.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Enabled:", GUILayout.Width(150));
                bool enabled = EditorGUILayout.Toggle(currentTarget.enabled);
                if (enabled != currentTarget.enabled)
                {
                    currentTarget.enabled = enabled;
                    window.QueueSave();
                }
            }
        }

        public override void Save()
        {

        }
    }
}