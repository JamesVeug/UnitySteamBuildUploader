namespace Wireframe
{
    /// <summary>
    /// Used by reflection
    /// </summary>
    internal partial class UnityCloudService : AService
    {
        public UnityCloudService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            reason = "";
            return true;
        }

        public override void ProjectSettingsGUI()
        {
            // None
        }
    }
}