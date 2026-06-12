using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    internal partial class ItchioService : AService
    {
        public override string ServiceName => "Itch.io";
        public override string[] SearchKeywords => new string[]{"itch.io", "itch", "game distribution", "game upload"};

        // SettingsProviders are registered under "Itchio", not the ServiceName "Itch.io"
        public override string PreferencesPath => "Preferences/Build Uploader/Services/Itchio";
        public override string ProjectSettingsPath => "Project/Build Uploader/Services/Itchio";

        public ItchioService()
        {
            // Needed for reflection
        }
        
        public override bool IsReadyToStartBuild(out GUIContent reason)
        {
            if (!Itchio.Enabled)
            {
                reason = DisabledServiceGUI;
                return false;
            }
            
            if (!Itchio.Instance.IsInitialized)
            {
                reason = PreferencesLink("Itch.io is not initialized", "Itch.io has not been setup. Either something isn't set correctly or is failing to setup correctly.");
                return false;
            }

            reason = null;
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            ItchioAppData configs = ItchioUIUtils.GetItchioBuildData(false);
            if (configs == null)
            {
                return false;
            }
            
            return configs.Users.Count > 0 && configs.Channels.Count > 0;
        }
    }
}