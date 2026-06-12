using UnityEngine;

namespace Wireframe
{
    internal partial class GithubService : AService
    {
        public override string ServiceName => "Github";
        public override string[] SearchKeywords => new string[]{"Git", "hub"};

        public GithubService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Github.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            if (string.IsNullOrEmpty(Github.Token))
            {
                reason = PreferencesLink("Github Token credentials is not set", "Token is required to authenticate with Github");
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            return true;
        }

        public override void ProjectSettingsGUI()
        {
            base.ProjectSettingsGUI();
        }
    }
}