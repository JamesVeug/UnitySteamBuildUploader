namespace Wireframe
{
    internal partial class ItchioService : AService
    {
        public ItchioService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!Itchio.Enabled)
            {
                reason = "Itch.io service is not enabled in Preferences";
                return false;
            }

            
            if (!Itchio.Instance.IsInitialized)
            {
                reason = "Itch.io is not initialized";
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