using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Wireframe
{
    internal class UnityCloudWindowTab : WindowTab
    {
        private const int AutoRefreshTime = 60;

        public override string TabName => "UnityCloud";
        public override bool Enabled => UnityCloud.Enabled;
        
        private UnityCloudTarget currentTarget;

        private GUIStyle m_titleStyle;
        private GUIStyle m_targetFoldoutStyle;
        private Vector2 m_scrollPosition;

        private Dictionary<string, UnityCloudBuildUI> m_cloudBuildToUIList;
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
                m_cloudBuildToUIList = new Dictionary<string, UnityCloudBuildUI>();
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

                UploaderWindow.Repaint();
            }

            double timeSinceLastRefresh = (DateTime.UtcNow - UnityCloudAPI.LastSyncDateTime).TotalSeconds;
            if (timeSinceLastRefresh > AutoRefreshTime || UnityCloudAPI.TotalSyncs == 0) // 5 minutes
            {
                if (UploaderWindow.CurrentTab == this || UnityCloudAPI.CloudBuildTargets == null)
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

            if (!DrawSettings())
            {
                return;
            }

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
                                        b = new UnityCloudBuildUI(cloudBuild);
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
            StartTargetBuildCoroutine(buildTargetID);
        }

        private async Task StartTargetBuildCoroutine(string buildTargetID)
        {
            UnityWebRequest request = UnityCloudAPI.StartBuild(buildTargetID);
            UnityWebRequestAsyncOperation webRequest = request.SendWebRequest();
            while (!webRequest.isDone)
            {
                await Task.Delay(10);
            }

            string downloadHandlerText = request.downloadHandler.text;
            if (request.isHttpError || request.isNetworkError)
            {
                string message = string.Format("Could not start build target '{0}' with UnityCloud:\nError: {1}",
                    buildTargetID, downloadHandlerText);
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Start new Build Failed", message, "OK");
                return;
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
                    using (new EditorGUI.DisabledScope(UnityCloudAPI.IsSyncing))
                    {
                        if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                        {
                            UnityCloudAPI.SyncBuilds();
                        }
                    }
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

        private bool DrawSettings()
        {
            if (string.IsNullOrEmpty(UnityCloud.Instance.Organization))
            {
                GUILayout.Label("No Organization set in settings. Please set this in Edit->Preferences->Build Uploader");
                return false;
            }
            else if (string.IsNullOrEmpty(UnityCloud.Instance.Project))
            {
                GUILayout.Label("No Project set in settings. Please set this in Edit->Preferences->Build Uploader");
                return false;
            }
            else if (string.IsNullOrEmpty(UnityCloud.Instance.Secret))
            {
                GUILayout.Label("No Secret set in settings. Please set this in Edit->Preferences->Build Uploader");
                return false;
            }

            return true;
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
                    UploaderWindow.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Name:", GUILayout.Width(150));
                string newTargetName = EditorGUILayout.TextField(currentTarget.name);
                if (newTargetName != currentTarget.name)
                {
                    currentTarget.name = newTargetName;
                    UploaderWindow.QueueSave();
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
                    UploaderWindow.QueueSave();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Enabled:", GUILayout.Width(150));
                bool enabled = EditorGUILayout.Toggle(currentTarget.enabled);
                if (enabled != currentTarget.enabled)
                {
                    currentTarget.enabled = enabled;
                    UploaderWindow.QueueSave();
                }
            }
        }

        public override void Save()
        {

        }
    }
}