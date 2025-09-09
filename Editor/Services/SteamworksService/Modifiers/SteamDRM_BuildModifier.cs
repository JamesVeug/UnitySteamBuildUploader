using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("Steam DRM", "modifiers", "Prevents your executable from being executed unless run from Steam by sending it to Steamworks.")]
    [UploadModifier("Steam DRM")]
    public partial class SteamDRM_BuildModifier : AUploadModifer
    {
        [Wiki("App", "The Steam App ID to use for the build. eg: 1141030")]
        private SteamApp m_app;
        
        [Wiki("Flags", "The flags to use when wrapping the build. default: 0. See https://partner.steamgames.com/doc/features/drm")]
        private int m_flags;
        
        public SteamDRM_BuildModifier()
        {
            // Required for reflection
            m_app = null;
            m_flags = 0;
        }

        /// <summary>
        /// https://partner.steamgames.com/doc/features/drm
        /// </summary>
        public SteamDRM_BuildModifier(int appID, int flags = 0)
        {
            m_app = new SteamApp()
            {
                App = new AppVDFFile()
                {
                    appid = appID
                }
            };
            m_flags = flags;
        }

        public override void TryGetErrors(UploadConfig config, List<string> errors)
        {
            base.TryGetErrors(config, errors);
            
            if (!InternalUtils.GetService<SteamworksService>().IsReadyToStartBuild(out string reason))
            {
                errors.Add(reason);
            }
            
            if (m_app == null)
            {
                errors.Add("No Steam App selected");
            }
        }

        public override async Task<bool> ModifyBuildAtPath(string cachedFolderPath, UploadConfig uploadConfig,
            int configIndex, UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            // Find .exe
            string[] files = System.IO.Directory.GetFiles(cachedFolderPath, "*.exe", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                stepResult.AddError("[Steam] No exe found to DRMWrap in " + cachedFolderPath);
                stepResult.SetFailed("No exe found to DRMWrap");
                return false;
            }
            
            string exePath = "";
            if (files.Length > 1)
            {
                exePath = files.First(a => !Utils.Contains(a, "UnityCrashHandler", StringComparison.OrdinalIgnoreCase));
                stepResult.AddWarning("[Steam] Multiple exes found in " + cachedFolderPath + ". Using " + exePath);
            }
            else
            {
                exePath = files[0];
            }
            
            if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath))
            {
                stepResult.AddError("[Steam] No exe found to DRMWrap in " + cachedFolderPath);
                stepResult.SetFailed("No exe found to DRMWrap");
                return false;
            }
            
            int processID = ProgressUtils.Start("Steam DRM Modifier", "Wrapping exe with Steam DRM");
            bool result = await SteamSDK.Instance.DRMWrap(m_app.App.appid, exePath, exePath, m_flags, stepResult);
            ProgressUtils.Remove(processID);
            return result;
        }

        public override void TryGetWarnings(AUploadDestination destination, List<string> warnings)
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