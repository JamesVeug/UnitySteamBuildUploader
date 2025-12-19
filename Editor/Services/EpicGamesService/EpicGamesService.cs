using System.Linq;

namespace Wireframe
{
    internal partial class EpicGamesService : AService
    {
        public override string ServiceName => "Epic Games";
        public override string[] SearchKeywords => new string[]{"epic", "games", "epicgames", "epic games", "unreal", "ue", "game distribution", "game upload"};

        public EpicGamesService()
        {
            // Needed for reflection
        }

        public override bool IsReadyToStartBuild(out string reason)
        {
            if (!EpicGames.Enabled)
            {
                reason = "Epic is not enabled in Preferences";
                return false;
            }

            if (string.IsNullOrEmpty(EpicGames.SDKPath))
            {
                reason = "Epic SDK Path is not set in Preferences";
                return false;
            }

            reason = string.Empty;
            
            return true;
        }

        public override bool IsProjectSettingsSetup()
        {
            EpicGamesAppData data = EpicGamesUIUtils.GetEpicGamesData(false);
            if (data == null)
            {
                return false;
            }
            
            return data.Organizations.Count > 0 && data.Organizations.Any(a=>a.Products.Count > 0);
        }

        public static string GetClientSecret(string organization, string clientID)
        {
            string key = "EpicGames_ClientSecret_" + organization + "_" + clientID + "S";
            return EncodedEditorPrefs.GetString(key, "");
        }

        public static void SetClientSecret(string organization, string clientID, string secret)
        {
            string key = "EpicGames_ClientSecret_" + organization + "_" + clientID + "S";
            EncodedEditorPrefs.SetString(key, secret);
        }
    }
}