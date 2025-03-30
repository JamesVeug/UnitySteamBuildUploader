using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildConfig
    {
        public bool Collapsed { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string GUID { get; set; }
        public List<ABuildConfigModifer> Modifiers => m_modifiers;

        private ABuildSource m_buildSource;
        private UIHelpers.BuildSourcesPopup.SourceData m_buildSourceType;
        
        private List<ABuildConfigModifer> m_modifiers = new List<ABuildConfigModifer>();
        
        private ABuildDestination m_buildDestination;
        private UIHelpers.BuildDestinationsPopup.DestinationData m_buildDestinationType;

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
            InitializeModifiers();
        }

        private void InitializeModifiers()
        {
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
            // Draw the build but on one line
            using (new EditorGUILayout.HorizontalScope())
            {
                // Source Type
                if (UIHelpers.SourcesPopup.DrawPopup(ref m_buildSourceType, GUILayout.MaxWidth(120)))
                {
                    isDirty = true;
                    if (m_buildSourceType != null)
                    {
                        m_buildSource = Activator.CreateInstance(m_buildSourceType.Type, new object[] { uploaderWindow }) as ABuildSource;
                    }
                    else
                    {
                        m_buildSource = null;
                    }
                }

                float splitWidth = 100;
                float maxWidth = m_window.position.width - splitWidth - 120;
                float parts = maxWidth / 2 - splitWidth;

                // Source
                float sourceWidth = parts;
                using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(sourceWidth)))
                {
                    if (m_buildSource != null)
                    {
                        m_buildSource.OnGUICollapsed(ref isDirty, sourceWidth);
                    }
                }


                // Progress
                string progressText = "->";
                if (IsBuilding())
                {
                    float progress = m_buildSource.DownloadProgress() + m_buildDestination.UploadProgress();
                    float ratio = progress / 2.0f;
                    int percentage = (int)(ratio * 100);
                    progressText = string.Format("{0}%", percentage);
                }

                GUILayout.Label(progressText, m_titleStyle, GUILayout.Width(splitWidth));

                // Destination Type
                if (UIHelpers.DestinationsPopup.DrawPopup(ref m_buildDestinationType))
                {
                    isDirty = true;
                    if(m_buildDestinationType != null){
                        m_buildDestination = Activator.CreateInstance(m_buildDestinationType.Type, new object[]{uploaderWindow}) as ABuildDestination;
                    }
                    else
                    {
                        m_buildDestination = null;
                    }
                }

                // Destination
                float destinationWidth = parts;
                using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(destinationWidth)))
                {
                    if (m_buildDestination != null)
                    {
                        m_buildDestination.OnGUICollapsed(ref isDirty, parts);
                    }
                }
            }
            
            List<string> warnings = GetAllWarnings();
            if(warnings.Count > 0)
            {
                foreach (string warning in warnings)
                {
                    DrawWarning(warning);
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
            foreach (ABuildConfigModifer modifer in m_modifiers)
            {
                modifer.TryGetWarnings(this, warnings);
                modifer.TryGetWarnings(m_buildSource, warnings);
                modifer.TryGetWarnings(m_buildDestination, warnings);
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
                    GUILayout.Label("Source", m_titleStyle);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Source Type: ", GUILayout.Width(120));
                        if (UIHelpers.SourcesPopup.DrawPopup(ref m_buildSourceType))
                        {
                            isDirty = true;
                            if (m_buildSourceType != null)
                            {
                                m_buildSource = Activator.CreateInstance(m_buildSourceType.Type, new object[]{uploaderWindow}) as ABuildSource;
                            }
                            else
                            {
                                m_buildSource = null;
                            }
                        }
                    }

                    if (m_buildSource != null)
                    {
                        m_buildSource.OnGUIExpanded(ref isDirty);
                        List<string> warnings = new List<string>();
                        foreach (ABuildConfigModifer modifer in m_modifiers)
                        {
                            modifer.TryGetWarnings(this, warnings);
                            modifer.TryGetWarnings(m_buildSource, warnings);
                            foreach (string warning in warnings)
                            {
                                DrawWarning(warning);
                            }
                        }
                    }
                
                    GUILayout.Space(10);
                    
                    // Modifiers
                    GUILayout.Label("Modifiers");
                    if (m_modifiers == null || m_modifiers.Count == 0)
                    {
                        InitializeModifiers();
                    }
                    foreach (ABuildConfigModifer modifer in m_modifiers)
                    {
                        isDirty |= modifer.OnGUI();
                    }
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth / 2)))
                {
                    GUILayout.Label("Destination", m_titleStyle);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Destination Type: ", GUILayout.Width(120));
                            if (UIHelpers.DestinationsPopup.DrawPopup(ref m_buildDestinationType))
                            {
                                isDirty = true;
                                if (m_buildDestinationType != null)
                                {
                                    m_buildDestination = Activator.CreateInstance(m_buildDestinationType.Type,
                                        new object[] { uploaderWindow }) as ABuildDestination;
                                }
                                else
                                {
                                    m_buildDestination = null;
                                }
                            }
                        }
                    }
                    
                    if (m_buildDestination != null)
                    {
                        m_buildDestination.OnGUIExpanded(ref isDirty);
                        
                        List<string> warnings = new List<string>();
                        foreach (ABuildConfigModifer modifer in m_modifiers)
                        {
                            modifer.TryGetWarnings(m_buildDestination, warnings);
                            foreach (string warning in warnings)
                            {
                                DrawWarning(warning);
                            }
                        }
                    }
                }
            }
        }

        public bool CanStartBuild(out string reason)
        {
            if (m_buildSource == null)
            {
                reason = "Source is not setup";
                return false;
            }

            if (!m_buildSource.IsSetup(out string sourceReason))
            {
                reason = "Source: " + sourceReason;
                return false;
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

            if (m_buildDestination == null)
            {
                reason = "No Destination specified";
                return false;
            }
            if (!m_buildDestination.IsSetup(out string destinationReason))
            {
                reason = "Destination: " + destinationReason;
                return false;
            }

            reason = "";
            return true;
        }

        public ABuildDestination Destination()
        {
            return m_buildDestination;
        }

        public ABuildSource Source()
        {
            return m_buildSource;
        }

        public Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["enabled"] = Enabled,
                ["guid"] = GUID,
                ["sourceFullType"] = m_buildSource?.GetType().FullName,
                ["source"] = m_buildSource?.Serialize(),
                ["modifiers"] = m_modifiers.Select(a =>
                {
                    Dictionary<string,object> dictionary = a.Serialize();
                    dictionary["$type"] = a.GetType().FullName;
                    return dictionary;
                }).ToList(),
                ["destinationFullType"] = m_buildDestination?.GetType().FullName,
                ["destination"] = m_buildDestination?.Serialize()
            };

            return data;
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            // Enabled
            object enabled;
            if (data.TryGetValue("enabled", out enabled))
            {
                Enabled = (bool)enabled;
            }
            
            // GUID
            if (data.TryGetValue("guid", out object guid))
            {
                GUID = (string)guid;
            }
            else
            {
                // Generate a new GUID
                GUID = Guid.NewGuid().ToString().Substring(0,5);
            }

            // Source
            if (data.TryGetValue("sourceFullType", out object sourceFullType) && sourceFullType != null)
            {
                string sourceFullPath = (string)sourceFullType;
                Type type = Type.GetType(sourceFullPath);
                if (type != null)
                {
                    m_buildSource = Activator.CreateInstance(type, new object[]{m_window}) as ABuildSource;
                    if (m_buildSource != null)
                    {
                        Dictionary<string, object> sourceDictionary = (Dictionary<string, object>)data["source"];
                        m_buildSource.Deserialize(sourceDictionary);
                        m_buildSourceType = UIHelpers.SourcesPopup.Values.FirstOrDefault(a => a.Type == type);
                    }
                }
            }
            
            // Modifiers
            if (data.TryGetValue("modifiers", out object modifiers))
            {
                m_modifiers = new List<ABuildConfigModifer>(); // Clear so we know its empty
                
                List<object> modifierList = (List<object>)modifiers;
                foreach (object modifier in modifierList)
                {
                    Dictionary<string, object> modifierDictionary = (Dictionary<string, object>)modifier;
                    if (modifierDictionary.TryGetValue("$type", out object modifierType))
                    {
                        Type type = Type.GetType((string)modifierType);
                        if (type != null)
                        {
                            ABuildConfigModifer buildConfigModifer = Activator.CreateInstance(type) as ABuildConfigModifer;
                            if (buildConfigModifer != null)
                            {
                                buildConfigModifer.Initialize(()=>m_window.Repaint());
                                buildConfigModifer.Deserialize(modifierDictionary);
                                m_modifiers.Add(buildConfigModifer);
                            }
                        }
                    }
                }
            }
            

            // Destination
            if (data.TryGetValue("destinationFullType", out object destinationFullType) && destinationFullType != null)
            {
                string destinationFullPath = (string)destinationFullType;
                Type type = Type.GetType(destinationFullPath);
                if (type != null)
                {
                    m_buildDestination = Activator.CreateInstance(type, new object[]{m_window}) as ABuildDestination;
                    if (m_buildDestination != null)
                    {
                        Dictionary<string, object> destinationDictionary = (Dictionary<string, object>)data["destination"];
                        m_buildDestination.Deserialize(destinationDictionary);
                        m_buildDestinationType = UIHelpers.DestinationsPopup.Values.FirstOrDefault(a => a.Type == type);
                    }
                }
            }
        }

        public bool IsBuilding()
        {
            return m_buildSource != null && m_buildDestination != null &&
                   (m_buildSource.IsRunning || m_buildDestination.IsRunning);
        }
    }
}