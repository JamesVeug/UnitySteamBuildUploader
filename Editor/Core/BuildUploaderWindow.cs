using System.Collections.Generic;
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
        private List<WindowTab> m_tabs;

        private float m_nextSaveDelta;
        private float m_lastEditDelta;
        private bool m_saveQueued;

        [MenuItem("Window/Build Uploader/Open Window", false, -100)]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            BuildUploaderWindow uploaderWindow = CreateWindow<BuildUploaderWindow>();
            uploaderWindow.titleContent = new GUIContent("Build Uploader", Utils.WindowIcon);
            uploaderWindow.InitializeTabs();
            uploaderWindow.Show();
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
                m_tabs = new List<WindowTab>();
                foreach (AService service in InternalUtils.AllServices())
                {
                    WindowTab type = service.WindowTabType;
                    if (type != null)
                    {
                        m_tabs.Add(type);
                    }
                }
                
                m_tabs.Add(new WindowBuildConfigsTab());
                m_tabs.Add(new WindowUploadTab());
                m_tabs.Add(new WindowTasksTab());

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
            using (new GUILayout.HorizontalScope())
            {
                foreach (WindowTab tab in m_tabs)
                {
                    if (tab.Enabled)
                    {
                        bool isActive = CurrentTab == tab;
                        GUIStyle style = new GUIStyle(EditorStyles.toolbarButton)
                        {
                            fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                            alignment = TextAnchor.MiddleCenter,
                            padding = new RectOffset(10, 10, 4, 4)
                        };
                        if (GUILayout.Toggle(isActive, tab.TabName, style))
                        {
                            currentTab = tab;
                        }
                    }
                }
            }
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

        public T SetTab<T>() where T : WindowTab
        {
            foreach (WindowTab tab in m_tabs)
            {
                if (tab is T t)
                {
                    currentTab = tab;
                    return t;
                }
            }
            
            Debug.LogWarning($"Tab of type {typeof(T).Name} not found.");
            return null;
        }
    }
}