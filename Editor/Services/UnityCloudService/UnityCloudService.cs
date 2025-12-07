namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class UnityCloudService : AService
    {
        public override string ServiceName => "Unity Cloud";
        public override string[] SearchKeywords => new string[]{"unity", "cloud", "unity cloud", "ci", "devops"};

        public UnityCloudService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!UnityCloud.Enabled)
            {
                reason = "Unity Cloud is not enabled in Preferences";
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Organization))
            {
                reason = "Organization is not set in Preferences";
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Project))
            {
                reason = "Project is not set in Preferences";
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Secret))
            {
                reason = "Secret is not set in Preferences";
                return false;
            }
            
            
            reason = "";
            return true;
        }

        public override void ProjectSettingsGUI()
        {
            // None
        }
    }
}