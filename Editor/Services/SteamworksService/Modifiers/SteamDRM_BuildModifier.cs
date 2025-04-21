using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamDRM_BuildModifier : ABuildConfigModifer
    {
        private bool m_enabled;
        private SteamApp m_current;
        private int m_flags;

        /// <summary>
        /// https://partner.steamgames.com/doc/features/drm
        /// </summary>
        public SteamDRM_BuildModifier(SteamApp app, int flags = 0)
        {
            m_current = app;
            m_flags = flags;
        }

        public override void Initialize(Action onChanged)
        {
            m_flags = 0;
        }

        public override bool IsSetup(out string reason)
        {
            if (!m_enabled)
            {
                reason = "";
                return true;
            }

            if (!InternalUtils.GetService<SteamworksService>().IsReadyToStartBuild(out reason))
            {
                return false;
            }
            
            if (m_current == null)
            {
                reason = "No Steam App selected";
                return false;
            }
            
            reason = "";
            return true;
        }

        public override async Task<UploadResult> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig, int buildIndex)
        {
            if (!m_enabled)
            {
                return UploadResult.Success();
            }
            
            // Find .exe
            string exePath = System.IO.Directory.GetFiles(cachedDirectory, "*.exe", System.IO.SearchOption.TopDirectoryOnly)[0];
            if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath))
            {
                Debug.LogError("[Steam] No exe found to DRMWrap in " + cachedDirectory);
                return UploadResult.Failed("No exe found to DRMWrap");
            }
            
            int processID = ProgressUtils.Start("Steam DRM Modifier", "Wrapping exe with Steam DRM");
            UploadResult result = await SteamSDK.Instance.DRMWrap(m_current.App.appid, exePath, exePath, m_flags);
            ProgressUtils.Remove(processID);
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
                
                GUILayout.Label("Steam DRM (Anti-piracy)", GUILayout.Width(150));
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
        
        public override void TryGetWarnings(ABuildDestination destination, List<string> warnings)
        {
            if (!m_enabled)
            {
                return;
            }
            
            if (!(destination is SteamUploadDestination) && !(destination is NoUploadDestination))
            {
                warnings.Add("Steam DRM is set but the build is destined for a non-steam location. The build won't be playable!");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["enabled"] = m_enabled,
                ["flags"] = m_flags,
                ["appID"] = m_current?.Id
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_enabled = (bool)data["enabled"];
            m_flags = Convert.ToInt32(data["flags"]);
            
            if (data.TryGetValue("appID", out object configIDString) && configIDString != null)
            {
                SteamApp[] buildConfigs = SteamUIUtils.ConfigPopup.Values;
                m_current = buildConfigs.FirstOrDefault(a=> a.Id == (long)configIDString);
            }
        }
    }
}