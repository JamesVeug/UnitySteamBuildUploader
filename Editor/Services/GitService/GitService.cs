using UnityEngine;

namespace Wireframe
{
    internal partial class GitService : AService
    {
        public override string ServiceName => "Git";
        public override string[] SearchKeywords => new string[]{"Git", "Source Control", "Version Control", "Branch", "Commit"};

        public GitService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Git.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }

            if (!Git.IsAvailable)
            {
                reason = PreferencesLink("Git executable could not be found",
                    "Install git, or set the path to it so the git formats can be read.");
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            return true;
        }
    }
}
