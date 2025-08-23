using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Wireframe
{
    internal class WindowBuildConfigsTab : WindowTab
    {
        public override string TabName => "Build Configs";

        private StringFormatter.Context m_context;


        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        private Vector2 m_scrollPosition;
        private bool m_isDirty;

        public override void Initialize(BuildUploaderWindow uploaderWindow)
        {
            base.Initialize(uploaderWindow);

            m_context = new StringFormatter.Context();
        }

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
        }

        public override void OnGUI()
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
                            buildConfig.OnGUI(UploaderWindow.position.width - 20, ref m_isDirty, m_context);
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

        public override void Save()
        {
            base.Save();
            BuildConfigsUIUtils.Save();
        }
    }
}