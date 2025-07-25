﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Partial class for GUI content
    /// TODO: Move to UIToolkit
    /// </summary>
    public partial class BuildConfig
    {
        internal bool Collapsed { get; set; } = true;
        
        private GUIStyle m_titleStyle;
        private BuildUploaderWindow m_window;

        internal BuildConfig(BuildUploaderWindow window) : this(Guid.NewGuid().ToString().Substring(0,5))
        {
            m_window = window;
        }

        public void SetupDefaults()
        {
            AddSource(new SourceData()
            {
                Enabled = true,
                Source = null,
                SourceType = null
            });
            
            AddDestination(new DestinationData()
            {
                Enabled = true,
                Destination = null,
                DestinationType = null
            });
            
            // All Unity builds include a X_BurstDebugInformation_DoNotShip folder
            // This isn't needed so add it as a default modifier
            ExcludeFoldersModifier regexBuildModifier = new ExcludeFoldersModifier();
            regexBuildModifier.Add(".*DoNotShip", true, false);
            AddModifier(new ModifierData(regexBuildModifier, true));
        }
        
        private void SetupGUI()
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
        }

        internal void OnGUI(ref bool isDirty, BuildUploaderWindow uploaderWindow)
        {
            SetupGUI();

            using (new EditorGUI.DisabledScope(!Enabled))
            {
                if (Collapsed)
                {
                    OnGUICollapsed(ref isDirty, uploaderWindow);
                }
                else
                {
                    OnGUIExpanded(ref isDirty, uploaderWindow);
                }
            }
        }

        private void OnGUICollapsed(ref bool isDirty, BuildUploaderWindow uploaderWindow)
        {
            float splitWidth = 100;
            float maxWidth = m_window.position.width - splitWidth - 120;
            float parts = maxWidth / 2 - splitWidth;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    foreach (SourceData source in m_buildSources)
                    {
                        if (!source.Enabled)
                        {
                            continue;
                        }
                        
                        // Draw the build but on one line
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Source Type
                            if (UIHelpers.SourcesPopup.DrawPopup(ref source.SourceType, GUILayout.MaxWidth(120)))
                            {
                                isDirty = true;
                                Utils.CreateInstance(source.SourceType?.Type, out source.Source);
                            }

                            // Source
                            float sourceWidth = parts;
                            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(sourceWidth)))
                            {
                                if (source.Source != null)
                                {
                                    source.Source.OnGUICollapsed(ref isDirty, sourceWidth);
                                }
                            }
                        }
                    }

                    List<string> sourceErrors = GetSourceErrors();
                    if (sourceErrors.Count > 0)
                    {
                        foreach (string error in sourceErrors)
                        {
                            DrawError(error);
                        }
                    }
                    
                    List<string> sourceWarnings = GetSourceWarnings();
                    if (sourceWarnings.Count > 0)
                    {
                        foreach (string warning in sourceWarnings)
                        {
                            DrawWarning(warning);
                        }
                    }
                }

                // Progress / Modifiers
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        string progressText = "->";
                        if (IsBuilding())
                        {
                            // TODO: Get the actual progress value from the task
                            float progress = 0;
                            List<SourceData> activeSources = m_buildSources.Where(a => a.Enabled).ToList();
                            foreach (SourceData sourceData in activeSources)
                            {
                                progress += sourceData.Source.DownloadProgress();
                            }

                            List<DestinationData> activeDestinations =
                                m_buildDestinations.Where(a => a.Enabled).ToList();
                            foreach (DestinationData destinationData in activeDestinations)
                            {
                                progress += destinationData.Destination.UploadProgress();
                            }

                            float ratio = progress / (activeSources.Count + activeDestinations.Count);
                            int percentage = (int)(ratio * 100);
                            progressText = string.Format("{0}%", percentage);
                        }

                        GUILayout.Label(progressText, m_titleStyle, GUILayout.Width(splitWidth));
                    }
                    
                    List<string> modifierErrors = GetModifierErrors();
                    if (modifierErrors.Count > 0)
                    {
                        foreach (string error in modifierErrors)
                        {
                            DrawError(error);
                        }
                    }
                    
                    List<string> modifierWarnings = GetModifierWarnings();
                    if (modifierWarnings.Count > 0)
                    {
                        foreach (string warning in modifierWarnings)
                        {
                            DrawWarning(warning);
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    foreach (DestinationData destinationData in m_buildDestinations)
                    {
                        if (!destinationData.Enabled)
                        {
                            continue;
                        }
                        
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Destination Type
                            if (UIHelpers.DestinationsPopup.DrawPopup(ref destinationData.DestinationType))
                            {
                                isDirty = true;
                                Utils.CreateInstance(destinationData.DestinationType?.Type, out destinationData.Destination);
                            }

                            // Destination
                            float destinationWidth = parts;
                            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(destinationWidth)))
                            {
                                if (destinationData.Destination != null)
                                {
                                    destinationData.Destination.OnGUICollapsed(ref isDirty, parts);
                                }
                            }
                        }
                    }
                    
                    List<string> destinationErrors = GetDestinationErrors();
                    if (destinationErrors.Count > 0)
                    {
                        foreach (string error in destinationErrors)
                        {
                            DrawError(error);
                        }
                    }
                    
                    List<string> destinationWarnings = GetDestinationWarnings();
                    if (destinationWarnings.Count > 0)
                    {
                        foreach (string warning in destinationWarnings)
                        {
                            DrawWarning(warning);
                        }
                    }
                }
            }
        }

        private static void DrawWarning(string warning)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Utils.WarningIcon, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                Color color = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label("Warning: " + warning, EditorStyles.helpBox);
                GUI.color = color;
            }
        }

        private static void DrawError(string error)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Utils.ErrorIcon, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                Color color = GUI.color;
                GUI.color = Color.red;
                GUILayout.Label("Error: " + error, EditorStyles.helpBox);
                GUI.color = color;
            }
        }

        private void OnGUIExpanded(ref bool isDirty, BuildUploaderWindow uploaderWindow)
        {
            float windowWidth = m_window.position.width;
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth/2)))
                {
                    GUILayout.Label("Sources", m_titleStyle);
                    for (var i = 0; i < m_buildSources.Count; i++)
                    {
                        var source = m_buildSources[i];
                        using (new GUILayout.HorizontalScope())
                        {
                            isDirty |= CustomToggle.DrawToggle(ref source.Enabled, GUILayout.Width(20));

                            using (new EditorGUI.DisabledScope(!source.Enabled))
                            {
                                GUILayout.Label("Source Type: ", GUILayout.Width(100));
                                if (UIHelpers.SourcesPopup.DrawPopup(ref source.SourceType))
                                {
                                    isDirty = true;
                                    Utils.CreateInstance(source.SourceType?.Type, out source.Source);
                                }
                            }

                            if (source.SourceType != null)
                            {
                                if (source.SourceType.Type.TryGetWikiLink(out string sourceURL))
                                {
                                    if (GUILayout.Button("?", GUILayout.Width(20)))
                                    {
                                        Application.OpenURL(sourceURL);
                                    }
                                }
                            }

                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                if (EditorUtility.DisplayDialog("Remove Source",
                                        "Are you sure you want to remove this source?",
                                        "Yes", "Oops No!"))
                                {
                                    m_buildSources.RemoveAt(i--);
                                    isDirty = true;
                                }
                            }
                        }

                        if (source.Source != null)
                        {
                            using (new EditorGUI.DisabledScope(!source.Enabled))
                            {
                                source.Source.OnGUIExpanded(ref isDirty);

                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Export Folder: ", GUILayout.Width(120));
                                    if (EditorUtils.FormatStringTextField(ref source.ExportFolder, ref source.ShowFormattedExportFolder))
                                    {
                                        isDirty = true;
                                    }
                                }

                                foreach (ModifierData modifer in m_modifiers)
                                {
                                    if (modifer.Modifier == null || !modifer.Enabled)
                                    {
                                        continue;
                                    }
                                    
                                    List<string> errors = new List<string>();
                                    modifer.Modifier.TryGetErrors(source.Source, errors);
                                    foreach (string warning in errors)
                                    {
                                        DrawError(warning);
                                    }
                                    
                                    List<string> warnings = new List<string>();
                                    modifer.Modifier.TryGetWarnings(source.Source, warnings);
                                    foreach (string warning in warnings)
                                    {
                                        DrawWarning(warning);
                                    }
                                }
                            }
                        }

                        if (i > 0)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Duplicate Files: ", GUILayout.Width(120));
                                var newHandler = (Utils.FileExistHandling)EditorGUILayout.EnumPopup(source.DuplicateFileHandling);
                                if (source.DuplicateFileHandling != newHandler)
                                {
                                    source.DuplicateFileHandling = newHandler;
                                    isDirty = true;
                                }
                            }
                        
                        }
                        
                        if (source.Source != null && source.Enabled)
                        {
                            List<string> errors = new List<string>();
                            source.Source.TryGetErrors(errors);
                            foreach (string error in errors)
                            {
                                DrawError(error);
                            }
                            
                            List<string> warnings = new List<string>();
                            source.Source.TryGetWarnings(warnings);
                            foreach (string warning in warnings)
                            {
                                DrawWarning(warning);
                            }
                        }
                        
                        GUILayout.Space(10);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add New Source"))
                        {
                            m_buildSources.Add(new SourceData()
                            {
                                Enabled = true,
                            });
                            isDirty = true;
                        }
                    }
                }

                // Modifiers
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth / 2)))
                {
                    GUILayout.Label("Modifiers", m_titleStyle);
                    for (var i = 0; i < m_modifiers.Count; i++)
                    {
                        ModifierData modifiers = m_modifiers[i];
                        using (new GUILayout.HorizontalScope())
                        {
                            isDirty |= CustomToggle.DrawToggle(ref modifiers.Enabled, GUILayout.Width(20));

                            using (new EditorGUI.DisabledScope(!modifiers.Enabled))
                            {
                                GUILayout.Label("Modifier Type: ", GUILayout.Width(100));
                                if (UIHelpers.ModifiersPopup.DrawPopup(ref modifiers.ModifierType))
                                {
                                    isDirty = true;
                                    Utils.CreateInstance(modifiers.ModifierType?.Type, out modifiers.Modifier);
                                }
                            }
                            
                            if (modifiers.ModifierType != null)
                            {
                                if (modifiers.ModifierType.Type.TryGetWikiLink(out string sourceURL))
                                {
                                    if (GUILayout.Button("?", GUILayout.Width(20)))
                                    {
                                        Application.OpenURL(sourceURL);
                                    }
                                }
                            }

                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                if (EditorUtility.DisplayDialog("Remove Modifier",
                                        "Are you sure you want to remove this Modifier?",
                                        "Yes", "Oops No!"))
                                {
                                    m_modifiers.RemoveAt(i--);
                                    isDirty = true;
                                }
                            }
                        }
                        
                        if (modifiers.Modifier != null)
                        {
                            using (new EditorGUI.DisabledScope(!modifiers.Enabled))
                            {
                                modifiers.Modifier.OnGUIExpanded(ref isDirty);
                            }


                            if (modifiers.Enabled)
                            {
                                List<string> errors = new List<string>();
                                modifiers.Modifier.TryGetErrors(this, errors);
                                foreach (string error in errors)
                                {
                                    DrawError(error);
                                }

                                List<string> warnings = new List<string>();
                                modifiers.Modifier.TryGetWarnings(warnings);
                                foreach (string warning in warnings)
                                {
                                    DrawWarning(warning);
                                }
                            }
                        }
                        
                        GUILayout.Space(10);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add New Modifier"))
                        {
                            m_modifiers.Add(new ModifierData()
                            {
                                Enabled = true,
                            });
                            isDirty = true;
                        }
                    }
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth / 2)))
                {
                    GUILayout.Label("Destinations", m_titleStyle);
                    for (var i = 0; i < m_buildDestinations.Count; i++)
                    {
                        var destinationData = m_buildDestinations[i];
                        using (new GUILayout.HorizontalScope())
                        {
                            isDirty |= CustomToggle.DrawToggle(ref destinationData.Enabled, GUILayout.Width(20));

                            GUILayout.Label("Destination Type: ", GUILayout.Width(120));
                            using (new EditorGUI.DisabledScope(!destinationData.Enabled))
                            {
                                if (UIHelpers.DestinationsPopup.DrawPopup(ref destinationData.DestinationType))
                                {
                                    isDirty = true;
                                    Utils.CreateInstance(destinationData.DestinationType?.Type, out destinationData.Destination);
                                }
                            }
                            
                            if (destinationData.DestinationType != null)
                            {
                                if (destinationData.DestinationType.Type.TryGetWikiLink(out string sourceURL))
                                {
                                    if (GUILayout.Button("?", GUILayout.Width(20)))
                                    {
                                        Application.OpenURL(sourceURL);
                                    }
                                }
                            }
                            
                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                if(EditorUtility.DisplayDialog("Remove Destination",
                                       "Are you sure you want to remove this destination?", 
                                       "Yes", "Oops No!"))
                                {
                                    m_buildDestinations.RemoveAt(i--);
                                    isDirty = true;
                                }
                            }
                        }

                        if (destinationData.Destination != null)
                        {
                            using (new EditorGUI.DisabledScope(!destinationData.Enabled))
                            {
                                destinationData.Destination.OnGUIExpanded(ref isDirty);

                                if (destinationData.Enabled)
                                {
                                    List<string> errors = new List<string>();
                                    destinationData.Destination.TryGetErrors(errors);
                                    foreach (string error in errors)
                                    {
                                        DrawError(error);
                                    }

                                    List<string> warnings = new List<string>();
                                    destinationData.Destination.TryGetWarnings(warnings);
                                    foreach (ModifierData modifier in m_modifiers)
                                    {
                                        if (modifier.ModifierType == null || !modifier.Enabled)
                                        {
                                            continue;
                                        }

                                        modifier.Modifier.TryGetWarnings(destinationData.Destination, warnings);
                                    }

                                    foreach (string warning in warnings)
                                    {
                                        DrawWarning(warning);
                                    }
                                }
                            }
                        }
                        
                        GUILayout.Space(10);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add New Destination"))
                        {
                            m_buildDestinations.Add(new DestinationData()
                            {
                                Enabled = true,
                            });
                            isDirty = true;
                        }
                    }
                }
            }
        }
    }
}