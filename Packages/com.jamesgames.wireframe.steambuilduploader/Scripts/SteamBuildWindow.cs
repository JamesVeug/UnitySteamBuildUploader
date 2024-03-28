using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildWindow : EditorWindow
    {
        private const float SAVE_TIME_DELAY = 60 * 5; // 5 minutes 
        private const float EDIT_TIME_BEFORE_SAVE = 10; // 10 seconds 

        public enum Tabs
        {
            SteamSDK,
            UnityCloud,
            Sync,
            Build,
        }

        public Tabs CurrentTab = Tabs.Sync;

        private SteamBuildWindowSteamSDKTab m_steamSDKTab;
        private SteamBuildWindowUnityCloudTab m_unityCloudTab;
        private SteamBuildWindowBuildTab m_buildTab;
        private SteamBuildWindowSyncTab m_syncTab;

        private float m_nextSaveDelta;
        private float m_lastEditDelta;
        private bool m_saveQueued;

        [MenuItem("Window/Steam Build Uploader")]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            SteamBuildWindow window = (SteamBuildWindow)GetWindow(typeof(SteamBuildWindow));
            window.RefreshTabs();
            window.Show();
        }

        private void Update()
        {
            RefreshTabs();
            m_steamSDKTab.Update();
            m_unityCloudTab.Update();
            m_buildTab.Update();
            m_syncTab.Update();

            // Save
            if (m_saveQueued)
            {
                m_nextSaveDelta += Time.deltaTime;
                m_lastEditDelta += Time.deltaTime;

                bool autoSave = m_nextSaveDelta >= SAVE_TIME_DELAY;
                bool haveNotEditedRecently = m_lastEditDelta >= EDIT_TIME_BEFORE_SAVE;
                if (autoSave && haveNotEditedRecently)
                {
                    bool isUpdatingAssets = EditorApplication.isUpdating;
                    bool isCompiling = EditorApplication.isCompiling;
                    bool canAutoSave = !isUpdatingAssets && !isCompiling;
                    if (canAutoSave)
                    {
                        ImmediateSave();
                    }
                }
            }
        }

        private void RefreshTabs()
        {
            if (m_steamSDKTab == null)
            {
                m_steamSDKTab = new SteamBuildWindowSteamSDKTab();
                m_steamSDKTab.Initialize(this);
            }

            if (m_unityCloudTab == null)
            {
                m_unityCloudTab = new SteamBuildWindowUnityCloudTab();
                m_unityCloudTab.Initialize(this);
            }

            if (m_buildTab == null)
            {
                m_buildTab = new SteamBuildWindowBuildTab();
                m_buildTab.Initialize(this);
            }

            if (m_syncTab == null)
            {
                m_syncTab = new SteamBuildWindowSyncTab();
                m_syncTab.Initialize(this);
            }
        }

        private void OnGUI()
        {
            RefreshTabs();

            // Tabs
            Color defaultColor = GUI.backgroundColor;
            using (new GUILayout.HorizontalScope())
            {
                GUI.backgroundColor = CurrentTab == Tabs.SteamSDK ? Color.gray : Color.white;
                if (GUILayout.Button("SteamSDK"))
                    CurrentTab = Tabs.SteamSDK;

                GUI.backgroundColor = CurrentTab == Tabs.UnityCloud ? Color.gray : Color.white;
                if (GUILayout.Button("UnityCloud"))
                    CurrentTab = Tabs.UnityCloud;

                GUI.backgroundColor = CurrentTab == Tabs.Sync ? Color.gray : Color.white;
                if (GUILayout.Button("Sync"))
                    CurrentTab = Tabs.Sync;

                GUI.backgroundColor = CurrentTab == Tabs.Build ? Color.gray : Color.white;
                if (GUILayout.Button("Build"))
                    CurrentTab = Tabs.Build;
            }

            GUI.backgroundColor = defaultColor;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                switch (CurrentTab)
                {
                    case Tabs.SteamSDK:
                        m_steamSDKTab.OnGUI();
                        break;
                    case Tabs.UnityCloud:
                        m_unityCloudTab.OnGUI();
                        break;
                    case Tabs.Build:
                        m_buildTab.OnGUI();
                        break;
                    case Tabs.Sync:
                        m_syncTab.OnGUI();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        void OnDestroy()
        {
            ImmediateSave();
        }

        private void ImmediateSave()
        {
            m_steamSDKTab?.Save();
            m_buildTab?.Save();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            m_nextSaveDelta = 0;
            m_saveQueued = false;
            Debug.Log("Steam Build Window Saved");
        }

        public void QueueSave()
        {
            m_nextSaveDelta = 0;
            m_saveQueued = true;
        }
    }
}