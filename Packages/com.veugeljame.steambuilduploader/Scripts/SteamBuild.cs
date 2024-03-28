using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuild
    {
        private enum SourceType
        {
            Manual,
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

        private ASteamBuildSource m_buildSource;
        private ASteamBuildDestination m_buildDestination;

        private SourceType m_currentSourceType = SourceType.UnityCloud;
        private DestinationType m_currentDestinationType = DestinationType.SteamWorks;
        private GUIStyle m_titleStyle;
        private SteamBuildWindow m_window;

        public SteamBuild(SteamBuildWindow window)
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

        public void OnGUI()
        {
            Setup();

            using (new EditorGUI.DisabledScope(!Enabled))
            {
                if (Collapsed)
                {
                    OnGUICollapsed();
                }
                else
                {
                    OnGUIExpanded();
                }
            }
        }

        private void OnGUICollapsed()
        {
            // Draw the build but on one line
            using (new GUILayout.HorizontalScope())
            {
                // Source Type
                SourceType sourceType = (SourceType)EditorGUILayout.EnumPopup(m_currentSourceType);
                if (sourceType != m_currentSourceType || m_buildSource == null)
                {
                    m_currentSourceType = sourceType;
                    CreateSourceFromType(sourceType);
                }

                float maxWidth = m_window.position.width;
                float splitWidth = 100;
                float parts = maxWidth / 2 - splitWidth;

                // Source
                float sourceWidth = parts;
                using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(sourceWidth)))
                {
                    m_buildSource.OnGUICollapsed();
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
                DestinationType destinationType = (DestinationType)EditorGUILayout.EnumPopup(m_currentDestinationType);
                if (destinationType != m_currentDestinationType || m_buildDestination == null)
                {
                    m_currentDestinationType = destinationType;
                    CreateDestinationFromType(destinationType);
                }

                // Destination
                float destinationWidth = parts;
                using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(destinationWidth)))
                {
                    m_buildDestination.OnGUICollapsed();
                }
            }
        }

        private void OnGUIExpanded()
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
                        SourceType type = (SourceType)EditorGUILayout.EnumPopup(m_currentSourceType);
                        if (type != m_currentSourceType || m_buildSource == null)
                        {
                            m_currentSourceType = type;
                            CreateSourceFromType(type);
                        }
                    }

                    m_buildSource.OnGUIExpanded();
                }

                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(windowWidth)))
                {
                    GUILayout.Label("Destination", m_titleStyle);
                    m_buildDestination.OnGUIExpanded();
                }
            }
        }

        private ASteamBuildSource CreateSourceFromType(SourceType type)
        {
            switch (type)
            {
                case SourceType.Manual:
                    m_buildSource = new SteamBuildManualSource(m_window);
                    break;
                case SourceType.UnityCloud:
                    m_buildSource = new SteamBuildUnityCloudSource(m_window);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return m_buildSource;
        }

        private ASteamBuildDestination CreateDestinationFromType(DestinationType type)
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

        public bool CanStartBuild()
        {
            if (m_buildSource == null || !m_buildSource.IsSetup())
            {
                return false;
            }

            if (m_buildDestination == null || !m_buildDestination.IsSetup())
            {
                return false;
            }

            if (!SteamSDK.Instance.IsInitialized)
            {
                return false;
            }

            if (string.IsNullOrEmpty(SteamSDK.Instance.UserName) ||
                string.IsNullOrEmpty(SteamSDK.Instance.UserPassword))
            {
                return false;
            }

            return true;
        }

        public ASteamBuildDestination Destination()
        {
            return m_buildDestination;
        }

        public ASteamBuildSource Source()
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

            JObject sourceJson = (JObject)data["source"];
            Dictionary<string, object> sourceDictionary = sourceJson.ToObject<Dictionary<string, object>>();

            ASteamBuildSource source = CreateSourceFromType(m_currentSourceType);
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
            JObject destinationJson = (JObject)data["destination"];
            Dictionary<string, object> destinationDictionary = destinationJson.ToObject<Dictionary<string, object>>();

            ASteamBuildDestination destination = CreateDestinationFromType(m_currentDestinationType);
            destination.Deserialize(destinationDictionary);
        }
    }
}