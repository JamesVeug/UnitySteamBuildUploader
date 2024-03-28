using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindowBuildTab : SteamBuildWindowTab
    {
        private SteamBuild m_singleBuild;

        private GUIStyle m_titleStyle;
        private string m_buildDescription;

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            if (m_singleBuild == null)
            {
                m_singleBuild = new SteamBuild(window);
            }
        }

        public override void OnGUI()
        {
            Setup();

            m_singleBuild.OnGUI();

            GUILayout.FlexibleSpace();

            // Description
            GUILayout.Label("Build", m_titleStyle);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Description:", GUILayout.Width(100));
                m_buildDescription = GUILayout.TextField(m_buildDescription);
            }

            GUILayout.Space(50);

            bool startButtonDisabled = !CanStartBuild();
            using (new EditorGUI.DisabledScope(startButtonDisabled))
            {
                if (GUILayout.Button("Download and Upload", GUILayout.Height(100)))
                {
                    EditorCoroutineUtility.StartCoroutine(DownloadAndUpload(), window);
                }
            }
        }

        private bool CanStartBuild()
        {
            if (string.IsNullOrEmpty(m_buildDescription))
            {
                return false;
            }

            return m_singleBuild != null && m_singleBuild.CanStartBuild();
        }

        private IEnumerator DownloadAndUpload()
        {
            List<SteamBuild> builds = new List<SteamBuild>();
            builds.Add(m_singleBuild);

            SteamWindowBuildProgressWindow buildProgressWindow =
                new SteamWindowBuildProgressWindow(builds, m_buildDescription);
            IEnumerator startProgress = buildProgressWindow.StartProgress();
            yield return startProgress;
        }
    }
}