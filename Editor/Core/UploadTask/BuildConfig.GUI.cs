using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class BuildConfig : DropdownElement
    {
        internal bool Collapsed { get; set; } = true;
        public int Id { get; set; }
        public string DisplayName => BuildName;
        
        private ReorderableListOfScenes m_sceneList = new ReorderableListOfScenes();
        private ReorderableListOfStrings m_definesList = new ReorderableListOfStrings();
        
        private GUIStyle m_titleStyle;
        private bool m_showFormattedBuildName = false;
        private bool m_showFormattedProductName = false;
        
        private void SetupGUI()
        {
            if (m_titleStyle == null)
            {
                m_sceneList.Initialize(Scenes, "Scenes", _ => { Save(); });
                m_definesList.Initialize(ExtraScriptingDefines, "Extra Script Defines", _ => { Save(); });
                
                m_titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        public void OnGUI(float width, ref bool dirty, StringFormatter.Context context)
        {
            SetupGUI();
            
            if (Collapsed)
            {
                OnGUICollapsed(width, ref dirty, context);
            }
            else
            {
                OnGUIExpanded(width, ref dirty, context);
            }
        }

        public void OnGUICollapsed(float width, ref bool dirty, StringFormatter.Context context)
        {
            // Draw the collapsed view of the BuildConfig
            string formattedBuildName = StringFormatter.FormatString(BuildName, context);
            GUILayout.Label(formattedBuildName, GUILayout.Width(width - 50));
        }

        public void OnGUIExpanded(float width, ref bool dirty, StringFormatter.Context context)
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(width)))
                {
                    // Draw the title of the BuildConfig
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Build Name:", GUILayout.Width(150));
                        if (EditorUtils.FormatStringTextField(ref BuildName, ref m_showFormattedBuildName, context))
                        {
                            dirty = true;
                        }
                        
                        GUILayout.FlexibleSpace();
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("GUID:");
                            GUILayout.Label(GUID);
                        }

                        if (GUILayout.Button("Apply Settings", GUILayout.Width(120)))
                        {
                            ApplySettings(context);
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Product Name:", GUILayout.Width(150));
                        if (EditorUtils.FormatStringTextField(ref ProductName, ref m_showFormattedProductName, context))
                        {
                            dirty = true;
                        }
                    }
                    
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Development Build:", GUILayout.Width(150));
                        bool newDevelopmentBuild = EditorGUILayout.Toggle(IsDevelopmentBuild);
                        if (newDevelopmentBuild != IsDevelopmentBuild)
                        {
                            IsDevelopmentBuild = newDevelopmentBuild;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Build Scripts Only:", GUILayout.Width(150));
                        bool newDevelopmentBuild = EditorGUILayout.Toggle(BuildScriptsOnly);
                        if (newDevelopmentBuild != BuildScriptsOnly)
                        {
                            BuildScriptsOnly = newDevelopmentBuild;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Script Debugging:", GUILayout.Width(150));
                        bool newDevelopmentBuild = EditorGUILayout.Toggle(AllowDebugging);
                        if (newDevelopmentBuild != AllowDebugging)
                        {
                            AllowDebugging = newDevelopmentBuild;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Autoconnect Profiler:", GUILayout.Width(150));
                        bool newDevelopmentBuild = EditorGUILayout.Toggle(ConnectProfiler);
                        if (newDevelopmentBuild != ConnectProfiler)
                        {
                            ConnectProfiler = newDevelopmentBuild;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Deep Profiling Support:", GUILayout.Width(150));
                        bool newDevelopmentBuild = EditorGUILayout.Toggle(EnableDeepProfilingSupport);
                        if (newDevelopmentBuild != EnableDeepProfilingSupport)
                        {
                            EnableDeepProfilingSupport = newDevelopmentBuild;
                            dirty = true;
                        }
                    }
                    
                    if (m_sceneList.OnGUI())
                    {
                        Save();
                    }
                    
                    if (m_definesList.OnGUI())
                    {
                        Save();
                    }
                    
                    
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Target Platform:", GUILayout.Width(150));
                        BuildTargetGroup newTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup(TargetPlatform);
                        if (newTargetGroup != TargetPlatform)
                        {
                            TargetPlatform = newTargetGroup;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Target Architecture:", GUILayout.Width(150));
                        Architecture newArchitecture = (Architecture)EditorGUILayout.EnumPopup(TargetArchitecture);
                        if (newArchitecture != TargetArchitecture)
                        {
                            TargetArchitecture = newArchitecture;
                            dirty = true;
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Stripping Level:", GUILayout.Width(150));
                        ManagedStrippingLevel newStrippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup(StrippingLevel);
                        if (newStrippingLevel != StrippingLevel)
                        {
                            StrippingLevel = newStrippingLevel;
                            dirty = true;
                        }
                    }
                    
                    
                }
            }
        }

        private void Save()
        {
            BuildConfigsUIUtils.Save();
        }
    }
}