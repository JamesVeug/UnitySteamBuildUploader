using UnityEngine;

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
        
        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!UnityCloud.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Organization))
            {
                reason = PreferencesLink("Organization is not set", "Required to connect to Unity Cloud.");
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Project))
            {
                reason = PreferencesLink("Unity Project is not set", "Required to connect to Unity Cloud.");
                return false;
            }
            
            if (string.IsNullOrEmpty(UnityCloud.Instance.Secret))
            {
                reason = PreferencesLink("Secret is not set", "Required to connect to Unity Cloud.");
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