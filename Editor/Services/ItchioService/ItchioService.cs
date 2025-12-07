namespace Wireframe
{
    internal partial class ItchioService : AService
    {
        public override string ServiceName => "Itch.io";
        public override string[] SearchKeywords => new string[]{"itch.io", "itch", "game distribution", "game upload"};

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
    }
}