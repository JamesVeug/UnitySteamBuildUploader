using System.Threading.Tasks;

namespace Wireframe
{
    internal partial class SteamworksService : IAuthenticatedService
    {
        public string AuthServiceName => "Steam";

        public bool CanAuthenticate => SteamSDK.Enabled &&
                                       SteamSDK.Instance.IsInitialized &&
                                       !string.IsNullOrEmpty(SteamSDK.UserName);

        public string AuthStatusMessage => SteamSDK.LastProbeMessage;

        public AuthStatus GetAuthStatus()
        {
            return SteamSDK.GetDisplayStatus();
        }

        public async Task VerifyAuthAsync()
        {
            await SteamSDK.ProbeLoginAsync();
        }

        public void StartAuthentication()
        {
            // SteamCMD is the only thing that can authorize the machine, and it has to be interactive -
            // the user has to type their password and answer Steam Guard themselves.
            SteamSDK.Instance.ShowConsole($"+login \"{SteamSDK.UserName}\"");
        }
    }
}
