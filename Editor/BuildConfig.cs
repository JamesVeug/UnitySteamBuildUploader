using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class BuildConfig
    {
        private enum SourceType
        {
            File,
            UnityCloud
        }

        private enum DestinationType
        {
            None,
            SteamWorks
        }

        public bool Collapsed { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public bool IsBuilding => m_buildSource.IsRunning || m_buildDestination.IsRunning;

        private ABuildSource m_buildSource;
        private ABuildDestination m_buildDestination;

        private SourceType m_currentSourceType = SourceType.File;
        private DestinationType m_currentDestinationType = DestinationType.SteamWorks;
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

            if (m_buildDestination == null)
            {
                m_buildDestination = new SteamUploadDestination(m_window);
            }
        }

        public void OnGUI(ref bool isDirty)
        {
            Setup();

            using (new EditorGUI.DisabledScope(!Enabled))
            {
                if (Collapsed)
                {
                    OnGUICollapsed(ref isDirty);
                }
                else
                {
                    OnGUIExpanded(ref isDirty);
                }
            }
        }

        private void OnGUICollapsed(ref bool isDirty)
        {
            // Draw the build but on one line
            using (new EditorGUILayout.HorizontalScope())
            {
                // Source Type
                SourceType sourceType = (SourceType)EditorGUILayout.EnumPopup(m_currentSourceType, GUILayout.Width(100));
                if (sourceType != m_currentSourceType || m_buildSource == null)
                {
                    if (m_buildSource != null)
                    {
                        isDirty = true;
                    }

                    m_currentSourceType = sourceType;
                    CreateSourceFromType(sourceType);
                }

                float splitWidth = 100;
                float maxWidth = m_window.position.width - splitWidth - 120;
                float parts = maxWidth / 2 - splitWidth;
                Debug.Log("Window: " + maxWidth + " part: " + parts);

                // Source
                float sourceWidth = parts;
                using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(sourceWidth)))
                {
                    m_buildSource.OnGUICollapsed(ref isDirty, sourceWidth);
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
                DestinationType destinationType = (DestinationType)EditorGUILayout.EnumPopup(m_currentDestinationType, GUILayout.Width(100));
                if (destinationType != m_currentDestinationType || m_buildDestination == null)
                {
                    if (m_buildDestination != null)
                    {
                        isDirty = true;
                    }
                    m_currentDestinationType = destinationType;
                    CreateDestinationFromType(destinationType);
                }

                // Destination
                float destinationWidth = parts;
                using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(destinationWidth)))
                {
                    m_buildDestination.OnGUICollapsed(ref isDirty);
                }
            }
        }

        private void OnGUIExpanded(ref bool isDirty)
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
                        SourceType type = (SourceType)EditorGUILayout.EnumPopup(m_currentSourceType, GUILayout.Width(100));
                        if (type != m_currentSourceType || m_buildSource == null)
                        {
                            isDirty = true;
                            m_currentSourceType = type;
                            CreateSourceFromType(type);
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

        private ABuildSource CreateSourceFromType(SourceType type)
        {
            switch (type)
            {
                case SourceType.File:
                    m_buildSource = new FileSource(m_window);
                    break;
                case SourceType.UnityCloud:
                    m_buildSource = new UnityCloudSource(m_window);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return m_buildSource;
        }

        private ABuildDestination CreateDestinationFromType(DestinationType type)
        {
            switch (type)
            {
                case DestinationType.None:
                    m_buildDestination = new NoUploadDestination(m_window);
                    break;
                case DestinationType.SteamWorks:
                    m_buildDestination = new SteamUploadDestination(m_window);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return m_buildDestination;
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

            if (!SteamSDK.Instance.IsInitialized)
            {
                reason = "Steam SDK is not initialized";
                return false;
            }

            if (string.IsNullOrEmpty(SteamSDK.UserName) ||
                string.IsNullOrEmpty(SteamSDK.UserPassword))
            {
                reason = "Steam SDK credentials are not set";
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
                ["sourceType"] = m_currentSourceType,
                ["source"] = m_buildSource.Serialize(),
                ["destinationType"] = m_currentDestinationType,
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
            m_currentSourceType = (SourceType)(long)data["sourceType"];

            Dictionary<string, object> sourceDictionary = (Dictionary<string, object>)data["source"];

            ABuildSource source = CreateSourceFromType(m_currentSourceType);
            source.Deserialize(sourceDictionary);

            // Destination
            object t;
            if (!data.TryGetValue("destinationType", out t))
            {
                t = (long)DestinationType.None;
            }

            //m_currentDestinationType = (DestinationType) data.TryGetValue("destinationType", out long t);
            m_currentDestinationType = (DestinationType)(long)t;

            // Destination
            Dictionary<string, object> destinationDictionary = (Dictionary<string, object>)data["destination"];

            ABuildDestination destination = CreateDestinationFromType(m_currentDestinationType);
            destination.Deserialize(destinationDictionary);
        }
    }
}