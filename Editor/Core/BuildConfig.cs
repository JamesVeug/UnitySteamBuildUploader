using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildConfig
    {
        public bool Collapsed { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public bool IsBuilding
        {
            get
            {
                return (m_buildSource != null && m_buildSource.IsRunning) ||
                       (m_buildDestination != null && m_buildDestination.IsRunning);
            }
        }

        private ABuildSource m_buildSource;
        private UIHelpers.BuildSourcesPopup.SourceData m_buildSourceType;
        
        private ABuildDestination m_buildDestination;
        private UIHelpers.BuildDestinationsPopup.DestinationData m_buildDestinationType;

        private GUIStyle m_titleStyle;
        private BuildUploaderWindow m_window;

        public BuildConfig(BuildUploaderWindow window)
        {
            m_window = window;
        }

        private void Setup()
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
                if (UIHelpers.SourcesPopup.DrawPopup(ref m_buildSourceType))
                {
                    isDirty = true;
                    m_buildSource = Activator.CreateInstance(m_buildSourceType.Type, new object[]{uploaderWindow}) as ABuildSource;
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
                if (IsBuilding)
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
                    m_buildDestination = Activator.CreateInstance(m_buildDestinationType.Type, new object[]{uploaderWindow}) as ABuildDestination;
                }
                // DestinationType destinationType = (DestinationType)EditorGUILayout.EnumPopup(m_currentDestinationType, GUILayout.Width(100));
                // if (destinationType != m_currentDestinationType || m_buildDestination == null)
                // {
                //     if (m_buildDestination != null)
                //     {
                //         isDirty = true;
                //     }
                //     m_currentDestinationType = destinationType;
                //     CreateDestinationFromType(destinationType);
                // }

                // Destination
                float destinationWidth = parts;
                using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(destinationWidth)))
                {
                    if (m_buildDestination != null)
                    {
                        m_buildDestination.OnGUICollapsed(ref isDirty);
                    }
                }
            }
        }

        private void OnGUIExpanded(ref bool isDirty, BuildUploaderWindow uploaderWindow)
        {
            float windowWidth = m_window.position.width;
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth)))
                {
                    GUILayout.Label("Source", m_titleStyle);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Source Type: ", GUILayout.Width(120));
                        if (UIHelpers.SourcesPopup.DrawPopup(ref m_buildSourceType))
                        {
                            isDirty = true;
                            m_buildSource = Activator.CreateInstance(m_buildSourceType.Type, new object[]{uploaderWindow}) as ABuildSource;
                        }
                    }

                    m_buildSource.OnGUIExpanded(ref isDirty);
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth)))
                {
                    GUILayout.Label("Destination", m_titleStyle);
                    m_buildDestination.OnGUIExpanded(ref isDirty);
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

            foreach (AService service in InternalUtils.AllServices())
            {
                if (!service.IsReadyToStartBuild(out reason))
                {
                    return false;
                }
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
                ["sourceFullType"] = m_buildSource.GetType().FullName,
                ["source"] = m_buildSource.Serialize(),
                ["destinationFullType"] = m_buildDestination.GetType().FullName,
                ["destination"] = m_buildDestination.Serialize()
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

            // Source
            if (data.TryGetValue("sourceFullType", out object sourceFullType))
            {
                string sourceFullPath = (string)sourceFullType;
                Type type = Type.GetType(sourceFullPath);
                if (type != null)
                {
                    Dictionary<string, object> sourceDictionary = (Dictionary<string, object>)data["source"];
                    ABuildSource source = Activator.CreateInstance(type, new object[]{m_window}) as ABuildSource;
                    source.Deserialize(sourceDictionary);
                }
            }

            // Destination
            if (data.TryGetValue("destinationFullType", out object destinationFullType))
            {
                string destinationFullPath = (string)destinationFullType;
                Type type = Type.GetType(destinationFullPath);
                if (type != null)
                {
                    Dictionary<string, object> destinationDictionary = (Dictionary<string, object>)data["destination"];
                    ABuildDestination destination = Activator.CreateInstance(type, new object[]{m_window}) as ABuildDestination;
                    destination.Deserialize(destinationDictionary);
                }
            }
        }
    }
}