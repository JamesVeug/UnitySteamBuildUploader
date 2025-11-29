using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Partial class for GUI content
    /// TODO: Move to UIToolkit
    /// </summary>
    public partial class UploadConfig
    {
        internal bool Collapsed { get; set; } = true;
        
        private GUIStyle m_titleStyle;
        private bool m_deferredIsDirty;

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
            
            AddDefaultModifiers();
        }

        private void AddDefaultModifiers()
        {
            // All Unity builds include a X_BurstDebugInformation_DoNotShip folder
            // This isn't needed so add it as a default modifier
            ExcludeFoldersModifier regexBuildModifier = new ExcludeFoldersModifier();
            regexBuildModifier.Add(".*DoNotShip", true, false);
            regexBuildModifier.Add(".*ButDontShipItWithYourGame", true, false);
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

        internal void OnGUI(float windowWidth, ref bool isDirty)
        {
            SetupGUI();

            using (new EditorGUI.DisabledScope(!Enabled))
            {
                if (Collapsed)
                {
                    OnGUICollapsed(windowWidth, ref isDirty);
                }
                else
                {
                    OnGUIExpanded(windowWidth, ref isDirty);
                }
            }
        }

        private void OnGUICollapsed(float windowWidth, ref bool isDirty)
        {
            float splitWidth = 100;
            float maxWidth = windowWidth - splitWidth - 120;
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
                            if (UIHelpers.SourcesPopup.DrawPopup(ref source.SourceType, m_context, GUILayout.MaxWidth(120)))
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
                                    source.Source.OnGUICollapsed(ref isDirty, sourceWidth, m_context);
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
                            if (UIHelpers.DestinationsPopup.DrawPopup(ref destinationData.DestinationType, m_context))
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
                                    destinationData.Destination.OnPreGUI(ref isDirty, m_context);
                                    destinationData.Destination.OnGUICollapsed(ref isDirty, parts, m_context);
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

        private void OnGUIExpanded(float windowWidth, ref bool isDirty)
        {
            if (m_deferredIsDirty)
            {
                isDirty = true;
                m_deferredIsDirty = false;
            }
            
            using (new GUILayout.HorizontalScope())
            {
                float maxWidth = windowWidth / 4;
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(maxWidth)))
                {
                    DrawExpandedSources(ref isDirty);
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(maxWidth)))
                {
                    DrawExpandedModifiers(ref isDirty);
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(maxWidth)))
                {
                    DrawExpandedDestinations(ref isDirty);
                }
                
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(maxWidth)))
                {
                    DrawExpandedActions(ref isDirty);
                }
            }
        }

        private void DrawExpandedSources(ref bool isDirty)
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
                        if (UIHelpers.SourcesPopup.DrawPopup(ref source.SourceType, m_context))
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
                            
                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove Source"), false, () =>
                        {
                            if (EditorUtility.DisplayDialog("Remove Source",
                                    "Are you sure you want to remove this source?",
                                    "Yes", "Oops No!"))
                            {
                                m_buildSources.Remove(source);
                                m_deferredIsDirty = true;
                            }
                        });
                        menu.ShowAsContext();
                    }
                }

                if (source.Source != null)
                {
                    using (new EditorGUI.DisabledScope(!source.Enabled))
                    {
                        source.Source.OnGUIExpanded(ref isDirty, m_context);

                        using (new GUILayout.HorizontalScope())
                        {
                            GUIContent subFolderContent = new GUIContent("Sub Folder: ", 
                                "A sub-path in the cached directory of which this source will be saved to before being modified and uploaded. Leave empty to save to the root folder.");
                            GUILayout.Label(subFolderContent, GUILayout.Width(120));
                            if (EditorUtils.FormatStringTextField(ref source.SubFolder, ref source.ShowFormattedExportFolder, m_context))
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
                        
                if (source.Enabled)
                {
                    if (source.Source != null)
                    {
                        List<string> errors = new List<string>();
                        source.Source.TryGetErrors(errors, m_context);
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
                    else
                    {
                        DrawError("No source selected");
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

        private void DrawExpandedModifiers(ref bool isDirty)
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
                        if (UIHelpers.ModifiersPopup.DrawPopup(ref modifiers.ModifierType, m_context))
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

                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Add Default Modifiers"), false, () =>
                        {
                            AddDefaultModifiers();
                            m_deferredIsDirty = true;
                        });
                                
                        menu.AddSeparator("");
                                
                        menu.AddItem(new GUIContent("Remove Modifier"), false, () =>
                        {
                            if (EditorUtility.DisplayDialog("Remove Modifier",
                                    "Are you sure you want to remove this Modifier?",
                                    "Yes", "Oops No!"))
                            {
                                m_modifiers.Remove(modifiers);
                                m_deferredIsDirty = true;
                            }
                        });
                        menu.ShowAsContext();
                    }
                }
                        
                if (modifiers.Modifier != null)
                {
                    using (new EditorGUI.DisabledScope(!modifiers.Enabled))
                    {
                        modifiers.Modifier.OnGUIExpanded(ref isDirty, m_context);
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
                        modifiers.Modifier.TryGetWarnings(this, warnings);
                        foreach (string warning in warnings)
                        {
                            DrawWarning(warning);
                        }
                    }
                }
                else if (modifiers.Enabled)
                {
                    DrawError("No modifier selected");
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

        private void DrawExpandedDestinations(ref bool isDirty)
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
                        if (UIHelpers.DestinationsPopup.DrawPopup(ref destinationData.DestinationType, m_context))
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
                            
                            
                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove Destination"), false, () =>
                        {
                            if(EditorUtility.DisplayDialog("Remove Destination",
                                   "Are you sure you want to remove this destination?", 
                                   "Yes", "Oops No!"))
                            {
                                m_buildDestinations.Remove(destinationData);
                                m_deferredIsDirty = true;
                            }
                        });
                        menu.ShowAsContext();
                    }
                }

                if (destinationData.Destination != null)
                {
                    using (new EditorGUI.DisabledScope(!destinationData.Enabled))
                    {
                        destinationData.Destination.OnPreGUI(ref isDirty, m_context);
                        destinationData.Destination.OnGUIExpanded(ref isDirty, m_context);

                        if (destinationData.Enabled)
                        {
                            List<string> errors = new List<string>();
                            destinationData.Destination.TryGetErrors(errors, m_context);
                            foreach (string error in errors)
                            {
                                DrawError(error);
                            }

                            List<string> warnings = new List<string>();
                            destinationData.Destination.TryGetWarnings(warnings, m_context);
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
                else if(destinationData.Enabled)
                {
                    DrawError("No destination selected");
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

        private void DrawExpandedActions(ref bool isDirty)
        {
            GUILayout.Label("Actions", m_titleStyle);
            for (var i = 0; i < m_postActions.Count; i++)
            {
                var actionData = m_postActions[i];
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Action Type: ", GUILayout.Width(120));
                    var status =
                        (PostUploadActionData.UploadCompleteStatus)EditorGUILayout.EnumPopup(actionData.WhenToExecute,
                            GUILayout.Width(100));
                    if (status != actionData.WhenToExecute)
                    {
                        actionData.WhenToExecute = status;
                        isDirty = true;
                    }

                    bool disabled = actionData.WhenToExecute == PostUploadActionData.UploadCompleteStatus.Never;
                    using (new EditorGUI.DisabledScope(disabled))
                    {
                        if (UIHelpers.ActionsPopup.DrawPopup(ref actionData.ActionType, m_context,
                                GUILayout.Width(200)))
                        {
                            isDirty = true;
                            Utils.CreateInstance(actionData.ActionType?.Type, out actionData.UploadAction);
                        }
                    }
                    
                    if (CustomSettingsIcon.OnGUI())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove Action"), false, () =>
                        {
                            if(EditorUtility.DisplayDialog("Remove Action",
                                   "Are you sure you want to remove this Post Action?", 
                                   "Yes", "Oops No!"))
                            {
                                m_postActions.Remove(actionData);
                                m_deferredIsDirty = true;
                            }
                        });
                        menu.ShowAsContext();
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    if (actionData.ActionType != null)
                    {
                        using (new EditorGUI.DisabledScope(actionData.WhenToExecute == PostUploadActionData.UploadCompleteStatus.Never))
                        {
                            actionData.UploadAction.OnGUIExpanded(ref isDirty, m_context);

                            if (actionData.WhenToExecute != PostUploadActionData.UploadCompleteStatus.Never)
                            {
                                List<string> errors = new List<string>();
                                actionData.UploadAction.TryGetErrors(errors, m_context);
                                foreach (string error in errors)
                                {
                                    DrawError(error);
                                }

                                List<string> warnings = new List<string>();
                                actionData.UploadAction.TryGetWarnings(warnings, m_context);
                                foreach (PostUploadActionData action in m_postActions)
                                {
                                    if (action.UploadAction == null || action.WhenToExecute ==
                                        PostUploadActionData.UploadCompleteStatus.Never)
                                    {
                                        continue;
                                    }

                                    action.UploadAction.TryGetWarnings(warnings, m_context);
                                }

                                foreach (string warning in warnings)
                                {
                                    DrawWarning(warning);
                                }
                            }
                        }
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add New Acton"))
                {
                    AddAction(new PostUploadActionData()
                    {
                        WhenToExecute = PostUploadActionData.UploadCompleteStatus.Always
                    });
                    isDirty = true;
                }
            }
        }
    }
}