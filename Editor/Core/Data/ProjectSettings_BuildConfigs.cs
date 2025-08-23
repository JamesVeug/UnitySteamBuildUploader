using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Wireframe
{
    /// <summary>
    /// Services tab for Project settings
    /// </summary>
    public partial class ProjectSettings_BuildConfigs : SettingsProvider
    {
        private int width
        {
            get
            {
                // Look away
                MethodInfo methodInfo = typeof(SettingsProvider).GetProperty("settingsWindow", BindingFlags.Instance | BindingFlags.NonPublic).GetMethod;
                var b = methodInfo.Invoke(this, null);
                var fieldInfo = GetFieldInHierarchy(b.GetType(), "m_SettingsPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                VisualElement window = fieldInfo.GetValue(b) as VisualElement;
                return (int)window.contentRect.width;
            }
        }
        
        static FieldInfo GetFieldInHierarchy(Type type, string fieldName, BindingFlags flags)
        {
            while (type != null)
            {
                FieldInfo fi = type.GetField(fieldName, flags | BindingFlags.DeclaredOnly);
                if (fi != null)
                    return fi;

                type = type.BaseType;
            }
            return null;
        }

        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        private bool m_isDirty;
        private Vector2 m_scrollPosition;
        private StringFormatter.Context m_context;

        private void Setup()
        {
            m_titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            m_subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            m_context = new StringFormatter.Context();
        }

        public void BuildConfigsGUI()
        {
            Setup();
            List<BuildConfig> buildConfigs = BuildConfigsUIUtils.GetBuildConfigs();

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label("Build Configs", m_titleStyle);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("New Build Config"))
                    {
                        BuildConfig newConfig = new BuildConfig();
                        newConfig.SetupDefaults();
                        buildConfigs.Add(newConfig);
                        m_isDirty = true;
                    }

                    if (!Preferences.AutoSaveUploadConfigsAfterChanges)
                    {
                        string text = m_isDirty ? "*Save" : "Save";
                        if (GUILayout.Button(text, GUILayout.Width(100)))
                        {
                            Save();
                        }
                    }

                    string dropdownText = m_isDirty ? "*" : "";
                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent(dropdownText + "Save"), false, Save);
                        menu.ShowAsContext();
                    }
                }

                // Builds to upload
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                for (int i = 0; i < buildConfigs.Count; i++)
                {
                    BuildConfig buildConfig = buildConfigs[i];
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (CustomSettingsIcon.OnGUI())
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Move Up"), false, () =>
                            {
                                int indexOf = buildConfigs.IndexOf(buildConfig);
                                if (indexOf > 0)
                                {
                                    buildConfigs.RemoveAt(indexOf);
                                    buildConfigs.Insert(indexOf - 1, buildConfig);
                                    buildConfig.Id--;
                                    buildConfigs[indexOf + 1].Id++;
                                    
                                    m_isDirty = true;
                                }
                            });

                            menu.AddItem(new GUIContent("Move Down"), false, () =>
                            {
                                int indexOf = buildConfigs.IndexOf(buildConfig);
                                if (indexOf < buildConfigs.Count - 1)
                                {
                                    buildConfigs.RemoveAt(indexOf);
                                    buildConfigs.Insert(indexOf + 1, buildConfig);
                                    buildConfig.Id++;
                                    buildConfigs[indexOf - 1].Id--;
                                    m_isDirty = true;
                                }
                            });

                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Remove Upload Config",
                                        "Are you sure you want to remove this Upload Config?", "Delete", "Cancel"))
                                {
                                    buildConfigs.Remove(buildConfig);
                                    m_isDirty = true;
                                }
                            });
                            menu.ShowAsContext();
                        }
                        
                        if (CustomFoldoutButton.OnGUI(buildConfig.Collapsed))
                        {
                            buildConfig.Collapsed = !buildConfig.Collapsed;
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            buildConfig.OnGUI(this. width - 20, ref m_isDirty, m_context);
                        }
                        
                    }
                }

                if (m_isDirty && Preferences.AutoSaveUploadConfigsAfterChanges)
                {
                    Save();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        public void Save()
        {
            BuildConfigsUIUtils.Save();
        }
    }
}