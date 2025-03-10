namespace Wireframe
{
    internal partial class GithubService : AService
    {
        public GithubService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Github.Enabled)
            {
                reason = "Github is not enabled in Preferences";
                return false;
            }

            if (string.IsNullOrEmpty(Github.Token))
            {
                reason = "Github Token credentials are not set in Preferences";
                return false;
            }

            reason = "";
            return true;
        }

        public override void ProjectSettingsGUI()
        {
            
        }
    }
}