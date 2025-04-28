using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("Steam DRM", "modifiers", "Prevents your executable from being executed unless run from Steam by sending it to Steamworks.")]
    [BuildModifier("Steam DRM")]
    public partial class SteamDRM_BuildModifier : ABuildConfigModifer
    {
        [Wiki("App", "The Steam App ID to use for the build. eg: 1141030")]
        private SteamApp m_app;
        
        [Wiki("Flags", "The flags to use when wrapping the build. default: 0. See https://partner.steamgames.com/doc/features/drm")]
        private int m_flags;

        /// <summary>
        /// https://partner.steamgames.com/doc/features/drm
        /// </summary>
        public SteamDRM_BuildModifier(SteamApp app, int flags = 0)
        {
            m_app = app;
            m_flags = flags;
        }
        
        public SteamDRM_BuildModifier()
        {
            m_app = null;
            m_flags = 0;
        }

        public override bool IsSetup(out string reason)
        {
            if (!InternalUtils.GetService<SteamworksService>().IsReadyToStartBuild(out reason))
            {
                return false;
            }
            
            if (m_app == null)
            {
                reason = "No Steam App selected";
                return false;
            }
            
            reason = "";
            return true;
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedDirectory, BuildConfig buildConfig,
            int buildIndex, BuildTaskReport.StepResult stepResult)
        {
            // Find .exe
            string exePath = System.IO.Directory.GetFiles(cachedDirectory, "*.exe", System.IO.SearchOption.TopDirectoryOnly)[0];
            if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath))
            {
                stepResult.AddError("[Steam] No exe found to DRMWrap in " + cachedDirectory);
                stepResult.SetFailed("No exe found to DRMWrap");
                return false;
            }
            
            int processID = ProgressUtils.Start("Steam DRM Modifier", "Wrapping exe with Steam DRM");
            bool result = await SteamSDK.Instance.DRMWrap(m_app.App.appid, exePath, exePath, m_flags, stepResult);
            ProgressUtils.Remove(processID);
            return result;
        }

        public override void TryGetWarnings(ABuildDestination destination, List<string> warnings)
        {
            if (!(destination is SteamUploadDestination) && !(destination is NoUploadDestination))
            {
                warnings.Add("Steam DRM is set but the build is destined for a non-steam location. The build won't be playable!");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["flags"] = m_flags,
                ["appID"] = m_app?.Id
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_flags = Convert.ToInt32(data["flags"]);
            
            if (data.TryGetValue("appID", out object configIDString) && configIDString != null)
            {
                SteamApp[] buildConfigs = SteamUIUtils.ConfigPopup.Values;
                m_app = buildConfigs.FirstOrDefault(a=> a.Id == (long)configIDString);
            }
        }
    }
}