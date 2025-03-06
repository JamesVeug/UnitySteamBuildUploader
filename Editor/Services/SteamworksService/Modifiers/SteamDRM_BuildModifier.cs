using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal class SteamDRM_BuildModifier : ABuildConfigModifer
    {
        private bool m_enabled;
        private SteamApp m_current;
        private int m_flags;

        public override void Setup(Action onChanged)
        {
            m_flags = 0;
        }

        public override async Task<UploadResult> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex)
        {
            if (!m_enabled)
            {
                return UploadResult.Success();
            }
            
            // Find .exe
            string exePath = System.IO.Directory.GetFiles(cachedDirectory, "*.exe", System.IO.SearchOption.TopDirectoryOnly)[0];
            // Debug.Log("[Steam] DRMWrapping " + exePath);
            if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath))
            {
                Debug.LogError("[Steam] No exe found to DRMWrap in " + cachedDirectory);
                return UploadResult.Failed("No exe found to DRMWrap");
            }
            
            int processID = ProgressUtils.Start("Steam DRM Modifier", "Wrapping exe with Steam DRM");
            UploadResult result = await SteamSDK.Instance.DRMWrap(m_current.Id, exePath, exePath, m_flags);
            ProgressUtils.Remove(processID);
            // Debug.Log("[Steam] DRMWrap done " + result.Successful + " " + result.FailReason);
            return result;
        }

        public override bool OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                bool isDirty = false;
                bool newEnabled = EditorGUILayout.Toggle(m_enabled, GUILayout.Width(20));
                if (newEnabled != m_enabled)
                {
                    m_enabled = newEnabled;
                    isDirty = true;
                }
                
                GUILayout.Label("Steam DRM", GUILayout.Width(70));
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://partner.steamgames.com/doc/features/drm");
                }

                GUILayout.Label(":", GUILayout.Width(10));

                SteamUIUtils.ConfigPopup.DrawPopup(ref m_current, GUILayout.Width(130));

                GUILayout.Label("Flags", GUILayout.Width(40));
                m_flags = EditorGUILayout.IntField(m_flags, GUILayout.Width(40));
                return isDirty;
            }

        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["enabled"] = m_enabled
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_enabled = (bool)data["enabled"];
        }
    }
}