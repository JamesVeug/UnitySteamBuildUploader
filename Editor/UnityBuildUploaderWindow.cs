using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildUploaderWindow : EditorWindow
    {
        private const float SAVE_TIME_DELAY = 60 * 5; // 5 minutes 
        private const float EDIT_TIME_BEFORE_SAVE = 10; // 10 seconds 

        public WindowTab CurrentTab => currentTab;

        private WindowTab currentTab;
        private WindowTab[] m_tabs;

        private float m_nextSaveDelta;
        private float m_lastEditDelta;
        private bool m_saveQueued;

        [MenuItem("Window/Build Uploader")]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            BuildUploaderWindow uploaderWindow = (BuildUploaderWindow)GetWindow(typeof(BuildUploaderWindow));
            uploaderWindow.titleContent = new GUIContent("Build Uploader", WindowIcon);
            uploaderWindow.InitializeTabs();
            uploaderWindow.Show();
        }
        
        public static Texture2D WindowIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/Icon.png";
                UnityEngine.Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(UnityEngine.Object));
                return loadAssetAtPath as Texture2D;
            }
        }

        private void Update()
        {
            InitializeTabs();
            
            foreach (WindowTab tab in m_tabs)
            {
                if (tab.Enabled)
                {
                    tab.Update();
                }
            }

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

        private void InitializeTabs()
        {
            if (m_tabs == null)
            {
                m_tabs = new WindowTab[]
                {
                    new UnityCloudWindowTab(),
                    new WindowUploadTab(),
                };

                foreach (WindowTab tab in m_tabs)
                {
                    tab.Initialize(this);
                }
                
                currentTab = m_tabs[0];
            }
        }

        private void OnGUI()
        {
            InitializeTabs();

            if (!currentTab.Enabled)
            {
                currentTab = m_tabs.FirstOrDefault(a => a.Enabled);
            }

            // Tabs
            if (m_tabs.Count(a=>a.Enabled) > 1)
            {
                DrawTabs();
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                CurrentTab.OnGUI();
            }
        }

        private void DrawTabs()
        {
            Color defaultColor = GUI.backgroundColor;
            using (new GUILayout.HorizontalScope())
            {
                foreach (WindowTab tab in m_tabs)
                {
                    if (tab.Enabled)
                    {
                        GUI.backgroundColor = CurrentTab == tab ? Color.gray : Color.white;
                        if (GUILayout.Button(tab.TabName))
                        {
                            currentTab = tab;
                        }
                    }
                }
            }
            GUI.backgroundColor = defaultColor;
        }

        void OnDestroy()
        {
            ImmediateSave();
        }

        private void ImmediateSave()
        {
            if (m_tabs == null)
            {
                // Docked and never initialized
                return;
            }
            
            foreach (WindowTab tab in m_tabs)
            {
                if (tab.Enabled)
                {
                    tab.Save();
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            m_nextSaveDelta = 0;
            m_saveQueued = false;
            Debug.Log("Build Uploader Window Saved");
        }

        public void QueueSave()
        {
            m_nextSaveDelta = 0;
            m_saveQueued = true;
        }
    }
}