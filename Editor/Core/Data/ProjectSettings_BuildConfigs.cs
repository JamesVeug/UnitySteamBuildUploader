using System;
using System.Collections.Generic;
using System.Linq;
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
        private Context m_context;

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

            m_context = new Context();
        }

        private void CreateBuildConfigs(bool clearedBuildConfig, bool? debugging = null)
        {
            BuildConfig newConfig = new BuildConfig();
            if (clearedBuildConfig)
            {
                newConfig.Clear();
                newConfig.NewGUID();
                newConfig.BuildName = "New Build";
                newConfig.ProductName = Application.productName;
            }
            else
            {
                newConfig.SetEditorSettings();
            }

            if (debugging.HasValue)
            {
                newConfig.SetDebuggingOn(debugging.Value);
                if (debugging.Value)
                {
                    newConfig.BuildName += " (Debugging On)";
                }
            }

            List<BuildConfig> buildConfigs = BuildConfigsUIUtils.GetBuildConfigs();
            newConfig.Id = buildConfigs.Max(a=>a.Id) + 1;
            buildConfigs.Add(newConfig);
            BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
            
            Save();
        }
        
        public void BuildConfigsGUI()
        {
            Setup();
            List<BuildConfig> buildConfigs = BuildConfigsUIUtils.GetBuildConfigs();

            using (new GUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("New Build Config"))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Empty Config"), false, () =>
                        {
                            CreateBuildConfigs(true);
                        });
                        menu.AddItem(new GUIContent("Use Editor Settings"), false, () =>
                        {
                            CreateBuildConfigs(false);
                        });
                        menu.AddItem(new GUIContent("Use Editor Settings (Debugging On)"), false, () =>
                        {
                            CreateBuildConfigs(false, true);
                        });
                        menu.AddItem(new GUIContent("Use Editor Settings (Debugging Off)"), false, () =>
                        {
                            CreateBuildConfigs(false, false);
                        });
                        menu.ShowAsContext();
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
                        
                        menu.AddSeparator("");
                        
                        
                        menu.AddItem(new GUIContent("Add Default Build Configs"), false, ()=>
                        {
                            BuildConfigsUIUtils.CreateDefaultConfigs();
                            BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                            m_isDirty = true;
                        });
                        
                        menu.AddSeparator("");
                        
                        menu.AddItem(new GUIContent(dropdownText + "Remove all Build Configs"), false, () =>
                        {
                            if (EditorUtility.DisplayDialog("Remove all Build Configs",
                                    "Are you sure you want to remove all build configs?" +
                                    "\n\nThis will remove them from Player Settings and can NOT be undone!", "Remove All", "No"))
                            {
                                BuildConfigsUIUtils.Clear();
                                BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                m_isDirty = true;
                            }
                        });
                        
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
                            menu.AddItem(new GUIContent("Set Debugging/On"), false, () =>
                            {
                                buildConfig.SetDebuggingOn(true);
                                m_isDirty = true;
                            });
                            
                            menu.AddItem(new GUIContent("Set Debugging/Off"), false, () =>
                            {
                                buildConfig.SetDebuggingOn(false);
                                m_isDirty = true;
                            });
                            
                            menu.AddSeparator("");
                            
                            menu.AddItem(new GUIContent("Move To Top"), false, () =>
                            {
                                int indexOf = buildConfigs.IndexOf(buildConfig);
                                if (indexOf > 0)
                                {
                                    buildConfigs.RemoveAt(indexOf);
                                    buildConfigs.Insert(0, buildConfig);
                                    for (int j = 0; j < buildConfigs.Count; j++)
                                    {
                                        buildConfigs[j].Id = j + 1;
                                    }
                                    
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddItem(new GUIContent("Move Up"), false, () =>
                            {
                                int indexOf = buildConfigs.IndexOf(buildConfig);
                                if (indexOf > 0)
                                {
                                    buildConfigs.RemoveAt(indexOf);
                                    buildConfigs.Insert(indexOf - 1, buildConfig);
                                    buildConfig.Id--;
                                    buildConfigs[indexOf + 1].Id++;
                                    
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
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
                                    
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                    m_isDirty = true;
                                }
                            });

                            menu.AddItem(new GUIContent("Move To Bottom"), false, () =>
                            {
                                int indexOf = buildConfigs.IndexOf(buildConfig);
                                if (indexOf < buildConfigs.Count - 1)
                                {
                                    buildConfigs.RemoveAt(indexOf);
                                    buildConfigs.Add(buildConfig);
                                    for (int j = 0; j < buildConfigs.Count; j++)
                                    {
                                        buildConfigs[j].Id = j + 1;
                                    }
                                    
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                    m_isDirty = true;
                                }
                            });

                            menu.AddSeparator("");
                            
                            menu.AddItem(new GUIContent("Duplicate"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Duplicate Build Config",
                                        "Are you sure you want to duplicate this Build Config?", "Duplicate", "Cancel"))
                                {
                                    Dictionary<string, object> serialize = buildConfig.Serialize();
                                    BuildConfig copy = new BuildConfig();
                                    copy.Deserialize(serialize);
                                    copy.Id = buildConfigs.Max(a=>a.Id) + 1;
                                    copy.NewGUID();
                                    copy.BuildName += " Copy";
                                    buildConfigs.Add(copy);
                                    
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddSeparator("");
                            
                            menu.AddItem(new GUIContent("Reset All Settings"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Reset all Build Config",
                                        "Are you sure you want to reset all settings in this Build Config?", "Reset", "Cancel"))
                                {
                                    buildConfig.Clear();
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
                                    m_isDirty = true;
                                }
                            });
                            
                            menu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Remove Build Config",
                                        "Are you sure you want to remove this Build Config?", "Delete", "Cancel"))
                                {
                                    buildConfigs.Remove(buildConfig);
                                    BuildConfigsUIUtils.BuildConfigsPopup.Refresh();
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
            m_isDirty = false;
            BuildConfigsUIUtils.Save();
        }
    }
}