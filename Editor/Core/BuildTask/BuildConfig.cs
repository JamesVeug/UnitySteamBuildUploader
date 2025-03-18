using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class BuildConfig
    {
        public bool Collapsed { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string GUID { get; set; }
        
        public List<SourceData> Sources => m_buildSources;
        public List<ABuildConfigModifer > Modifiers => m_modifiers;
        public List<DestinationData> Destinations => m_buildDestinations;

        private List<SourceData> m_buildSources;
        private List<ABuildConfigModifer> m_modifiers;
        private List<DestinationData> m_buildDestinations;

        private GUIStyle m_titleStyle;
        private BuildUploaderWindow m_window;

        public BuildConfig(BuildUploaderWindow window)
        {
            m_window = window;
            GUID = Guid.NewGuid().ToString().Substring(0,5);
            Initialize();
        }

        private void Initialize()
        {
            m_buildSources = new List<SourceData>();
            m_buildSources.Add(new SourceData()
            {
                Enabled = true,
            });
            
            // All Unity builds include a X_BurstDebugInformation_DoNotShip folder
            // This isn't needed so add it as a default modifier
            ExcludeFilesByRegex_BuildModifier regexBuildModifier = new ExcludeFilesByRegex_BuildModifier();
            regexBuildModifier.Add("*_DoNotShip", true, false);
            
            m_modifiers = new List<ABuildConfigModifer>
            {
                regexBuildModifier,
                new SteamDRM_BuildModifier(),
            };
            
            foreach (ABuildConfigModifer modifer in m_modifiers)
            {
                modifer.Initialize(()=>m_window.Repaint());
            }
            
            m_buildDestinations = new List<DestinationData>();
            m_buildDestinations.Add(new DestinationData()
            {
                Enabled = true,
            });
        }

        public void Setup()
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

        public void OnGUI(ref bool isDirty, BuildUploaderWindow uploaderWindow)
        {
            Setup();

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
                                if (source.SourceType != null)
                                {
                                    source.Source = Activator.CreateInstance(source.SourceType.Type, new object[] { uploaderWindow }) as ABuildSource;
                                }
                                else
                                {
                                    source.Source = null;
                                }
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
                }

                // Progress
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

                    List<DestinationData> activeDestinations = m_buildDestinations.Where(a => a.Enabled).ToList();
                    foreach (DestinationData destinationData in activeDestinations)
                    {
                        progress += destinationData.Destination.UploadProgress();
                    }

                    float ratio = progress / (activeSources.Count + activeDestinations.Count);
                    int percentage = (int)(ratio * 100);
                    progressText = string.Format("{0}%", percentage);
                }

                GUILayout.Label(progressText, m_titleStyle, GUILayout.Width(splitWidth));

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
                                if (destinationData.DestinationType != null)
                                {
                                    destinationData.Destination =
                                        Activator.CreateInstance(destinationData.DestinationType.Type,
                                            new object[] { uploaderWindow }) as ABuildDestination;
                                }
                                else
                                {
                                    destinationData.Destination = null;
                                }
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

                    List<string> warnings = GetAllWarnings();
                    if (warnings.Count > 0)
                    {
                        foreach (string warning in warnings)
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

        private List<string> GetAllWarnings()
        {
            List<string> warnings = new List<string>();
            warnings.AddRange(GetSourceWarnings());
            warnings.AddRange(GetDestinationWarnings());

            return warnings;
        }

        private List<string> GetSourceWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (ABuildConfigModifer modifer in m_modifiers)
            {
                foreach (SourceData sourceData in m_buildSources)
                {
                    modifer.TryGetWarnings(sourceData.Source, warnings);
                }
            }
            
            return warnings;
        }

        private List<string> GetDestinationWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (ABuildConfigModifer modifier in m_modifiers)
            {
                foreach (DestinationData destinationData in m_buildDestinations)
                {
                    modifier.TryGetWarnings(destinationData.Destination, warnings);
                }
            }
            
            return warnings;
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
                                    if (source.SourceType != null)
                                    {
                                        source.Source =
                                            Activator.CreateInstance(source.SourceType.Type,
                                                new object[] { uploaderWindow }) as ABuildSource;
                                    }
                                    else
                                    {
                                        source.Source = null;
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
                                List<string> warnings = new List<string>();
                                foreach (ABuildConfigModifer modifer in m_modifiers)
                                {
                                    modifer.TryGetWarnings(this, warnings);
                                    modifer.TryGetWarnings(source.Source, warnings);
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
                    GUILayout.Label("Modifiers");
                    foreach (ABuildConfigModifer modifer in m_modifiers)
                    {
                        isDirty |= modifer.OnGUI();
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
                                    if (destinationData.DestinationType != null)
                                    {
                                        destinationData.Destination = Activator.CreateInstance(
                                            destinationData.DestinationType.Type,
                                            new object[] { uploaderWindow }) as ABuildDestination;
                                    }
                                    else
                                    {
                                        destinationData.Destination = null;
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

                                List<string> warnings = new List<string>();
                                foreach (ABuildConfigModifer modifier in m_modifiers)
                                {
                                    modifier.TryGetWarnings(destinationData.Destination, warnings);
                                }

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

        public bool CanStartBuild(out string reason)
        {
            if(m_buildSources.Count == 0)
            {
                reason = "No Sources specified";
                return false;
            }
            
            for (var i = 0; i < m_buildSources.Count; i++)
            {
                var source = m_buildSources[i];
                if (!source.Enabled)
                {
                    continue;
                }
                
                if (source.Source == null)
                {
                    reason = $"Source #{i+1} is not setup";
                    return false;
                }

                if (!source.Source.IsSetup(out string sourceReason))
                {
                    reason = $"Source #{i+1}: " + sourceReason;
                    return false;
                }
            }
            
            for (var i = 0; i < m_modifiers.Count; i++)
            {
                var modifier = m_modifiers[i];
                if (!modifier.IsSetup(out string modifierReason))
                {
                    reason = $"Modifier #{i+1}: {modifierReason}";
                    return false;
                }
            }

            if (m_buildDestinations.Count == 0)
            {
                reason = "No Destination specified";
                return false;
            }
            
            for (var i = 0; i < m_buildDestinations.Count; i++)
            {
                var destination = m_buildDestinations[i];
                if (!destination.Enabled)
                {
                    continue;
                }
                
                if (destination.Destination == null)
                {
                    reason = $"Destination #{i+1} is not setup";
                    return false;
                }

                if (!destination.Destination.IsSetup(out string destinationReason))
                {
                    reason = $"Destination #{i+1}: " + destinationReason;
                    return false;
                }
            }

            reason = "";
            return true;
        }

        public bool IsBuilding()
        {
            foreach (SourceData source in m_buildSources)
            {
                if (source.Source == null)
                {
                    return false;
                }
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                if (destination.Destination == null)
                {
                    return false;
                }
            }
            
            foreach (SourceData source in m_buildSources)
            {
                if (source.Source.IsRunning)
                {
                    return true;
                }
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                if (destination.Destination.IsRunning)
                {
                    return true;
                }
            }

            return false;
        }

        public void CleanUp()
        {
            foreach (SourceData source in m_buildSources)
            {
                source.Source?.CleanUp();
            }

            foreach (DestinationData destination in m_buildDestinations)
            {
                destination.Destination?.CleanUp();
            }
        }
    }
}