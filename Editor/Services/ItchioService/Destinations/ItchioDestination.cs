using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("Itchio", "destinations", "Upload a file or folder for a game.")]
    [UploadDestination("Itchio")]
    public partial class ItchioDestination : AUploadDestination
    {
        [Wiki("User", "Account that owns owns the game.")]
        private ItchioUser m_user;
        
        [Wiki("Game", "ID of the game as seen in the URL.")]
        private ItchioGameData m_game;
        
        [Wiki("Channels", "Which platforms to upload to (lower case). eg: windows,mac,linux,android.")]
        private List<ItchioChannel> m_channels;

        public ItchioDestination() : base()
        {
            // Required for reflection
            m_channels = new List<ItchioChannel>();
        }
        
        public ItchioDestination(string user, string game, string[] channels=null) : this()
        {
            m_user = new ItchioUser(){Name = user};
            m_game = new ItchioGameData(){Name = game};
            SetChannels(channels);
        }
        
        public void SetGame(string user, string game)
        {
            m_user = new ItchioUser(){Name = user};
            m_game = new ItchioGameData(){Name = game};
        }
        
        public void SetChannels(params string[] channels)
        {
            if(channels == null || channels.Length == 0)
            {
                // Default channels if none specified
                m_channels = new List<ItchioChannel>();
            }
            else
            {
                m_channels = channels.Select(c => new ItchioChannel(){Name = c}).ToList();
            }
        }

        public override async Task<bool> Upload(UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            string filePath = StringFormatter.FormatString(m_filePath, ctx);

            string user = StringFormatter.FormatString(m_user.Name, ctx);
            string game = StringFormatter.FormatString(m_game.Name, ctx);
            string version = m_buildDescription;
            List<string> channels = m_channels.ConvertAll((a)=>StringFormatter.FormatString(a.Name, ctx));
            
            int processID = ProgressUtils.Start("Itchio", "Uploading to Itchio");
            bool success = await Itchio.Instance.Upload(filePath, user, game, channels, version, result);
            ProgressUtils.Remove(processID);
            
            return success;
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);
            
            if (!InternalUtils.GetService<ItchioService>().IsReadyToStartBuild(out string serviceReason))
            {
                errors.Add(serviceReason);
            }
            
            if (m_user == null || string.IsNullOrEmpty(m_user.Name))
            {
                errors.Add("User not specified");
            }

            if (m_game == null || string.IsNullOrEmpty(m_game.Name))
            {
                errors.Add("Game not specified");
            }
            
            if (m_channels.Count == 0)
            {
                errors.Add("No channels specified");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                ["user"] = m_user?.Id,
                ["game"] = m_game?.Id,
                ["channels"] = m_channels.Select(a=>a.Id).ToList()
            };
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            ItchioUser[] users = ItchioUIUtils.UserPopup.Values;
            if (s.TryGetValue("user", out object user))
            {
                m_user = users.FirstOrDefault(a=>a.Id == (long)user);
            }
            
            if (s.TryGetValue("game", out object game) && m_user != null)
            {
                m_game = m_user.GameIds.FirstOrDefault(a=>a.ID == (long)game);
            }

            if (s.TryGetValue("channels", out object channels))
            {
                List<ItchioChannel> allChannels = ItchioUIUtils.GetItchioBuildData().Channels;
                List<long> channelNames = ((List<object>)channels).Select(a=>(long)a).ToList();
                m_channels = allChannels.Where(c => channelNames.Contains(c.ID)).ToList();
            }
        }
    }
}