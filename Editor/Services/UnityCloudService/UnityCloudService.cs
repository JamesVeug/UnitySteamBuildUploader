namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class UnityCloudService : AService
    {
        public override WindowTab WindowTabType => new UnityCloudWindowTab();
        
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