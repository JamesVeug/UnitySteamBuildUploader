using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Adds an already-uploaded Apple build to one or more TestFlight beta groups.
    ///
    /// Consumes a Build ID via a format string (defaults to {appleBuildId}) produced by
    /// an upstream AppleUploadDestination. Calls
    /// POST /v1/betaGroups/{id}/relationships/builds for each selected group.
    ///
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(AppleAddToTestFlightGroupAction), "actions",
        "Add an uploaded Apple build to one or more TestFlight beta groups.")]
    [UploadAction("Apple Add To TestFlight Group")]
    public partial class AppleAddToTestFlightGroupAction : AUploadAction
    {
        [Wiki("API Key", "Which App Store Connect API Key to authenticate with.", 1)]
        private AppleConfig.AppleApiKey m_apiKey;

        [Wiki("App", "Which App Store Connect app the beta groups belong to.", 2)]
        private AppleConfig.AppleApp m_app;

        [Wiki("Beta Groups", "Which TestFlight beta groups to add the build to.", 3)]
        private List<AppleConfig.AppleBetaGroup> m_betaGroups;

        [Wiki("Build ID Format",
            "Format string that resolves to the App Store Connect Build resource ID. " +
            "Defaults to {appleBuildId} which is populated by an upstream Apple TestFlight upload step.", 4)]
        private string m_buildIdFormat = Context.APPLE_BUILD_ID_KEY;

        public AppleAddToTestFlightGroupAction() : base()
        {
            // Required for reflection
            m_betaGroups = new List<AppleConfig.AppleBetaGroup>();
        }

        public override async Task<bool> Execute(UploadTaskReport.StepResult stepResult)
        {
            string buildId = m_context.FormatString(m_buildIdFormat);
            if (string.IsNullOrEmpty(buildId) || buildId == "???")
            {
                stepResult.SetFailed(
                    "Build ID format did not resolve to a value. Make sure an Apple TestFlight upload step ran successfully earlier in the pipeline.");
                return false;
            }

            bool allSucceeded = true;
            foreach (AppleConfig.AppleBetaGroup group in m_betaGroups)
            {
                if (group == null || string.IsNullOrEmpty(group.BetaGroupID))
                {
                    stepResult.AddError($"Beta group '{group?.Name}' has no Beta Group ID; skipping.");
                    allSucceeded = false;
                    continue;
                }

                AppleSimpleResponse response = await Apple.AddBuildToBetaGroup(
                    m_apiKey, group.BetaGroupID, buildId, stepResult);

                if (!response.Successful)
                {
                    allSucceeded = false;
                }
            }

            return allSucceeded;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);

            if (!Apple.Enabled)
            {
                errors.Add("Apple is not enabled. Enable it in Preferences -> Build Uploader -> Services -> Apple.");
            }

            if (m_apiKey == null)
            {
                errors.Add("API Key is not set.");
            }
            else if (string.IsNullOrEmpty(m_apiKey.PrivateKeyPath))
            {
                errors.Add($"API Key '{m_apiKey.Name}' has no .p8 file path. Set it in Preferences.");
            }

            if (m_app == null)
            {
                errors.Add("App is not set.");
            }

            if (m_betaGroups == null || m_betaGroups.Count == 0)
            {
                errors.Add("No beta groups selected.");
            }
            else
            {
                foreach (var group in m_betaGroups)
                {
                    if (group == null || string.IsNullOrEmpty(group.BetaGroupID))
                    {
                        errors.Add($"Beta group '{group?.Name}' has no Beta Group ID.");
                    }
                }
            }

            if (string.IsNullOrEmpty(m_buildIdFormat))
            {
                errors.Add("Build ID format is empty.");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "apiKey", m_apiKey?.Id ?? 0 },
                { "app", m_app?.Id ?? 0 },
                { "betaGroups", m_betaGroups.Select(g => g.Id).ToList() },
                { "buildIdFormat", m_buildIdFormat ?? "" }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            AppleConfig.AppleApiKey[] keys = AppleUIUtils.ApiKeyPopup.Values;
            if (data.TryGetValue("apiKey", out object apiKeyId) && apiKeyId != null)
            {
                m_apiKey = keys.FirstOrDefault(a => a.Id == (long)apiKeyId);
            }

            AppleConfig.AppleApp[] apps = AppleUIUtils.AppPopup.Values;
            if (data.TryGetValue("app", out object appId) && appId != null)
            {
                m_app = apps.FirstOrDefault(a => a.Id == (long)appId);
            }

            m_betaGroups = new List<AppleConfig.AppleBetaGroup>();
            if (m_app != null && data.TryGetValue("betaGroups", out object groupsObj) && groupsObj is List<object> groupIds)
            {
                List<long> ids = groupIds.Select(o => (long)o).ToList();
                m_betaGroups = m_app.betaGroups.Where(g => ids.Contains(g.Id)).ToList();
            }

            m_buildIdFormat = data.TryGetValue("buildIdFormat", out object bf) ? bf?.ToString() ?? Context.APPLE_BUILD_ID_KEY : Context.APPLE_BUILD_ID_KEY;
        }
    }
}
