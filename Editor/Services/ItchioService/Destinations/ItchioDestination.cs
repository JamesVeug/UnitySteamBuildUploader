using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wireframe
{
    [Wiki("Itchio", "destinations", "Upload a file or folder for a game.")]
    [BuildDestination("Itchio")]
    public partial class ItchioDestination : ABuildDestination
    {
        [Wiki("User", "Account that owns owns the game.")]
        private string m_user;
        
        [Wiki("Game", "ID of the game as seen in the URL.")]
        private string m_game;
        
        [Wiki("Channels", "Which platforms to upload to (lower case). eg: windows,mac,linux,android.")]
        private List<string> m_channels;
        
        [Wiki("Version", "What version this build is. eg: 1.0.0")]
        private string m_version;

        public ItchioDestination() : base()
        {
            // Required for reflection
            m_channels = new List<string>();
            m_version = "{version}";
        }
        
        public ItchioDestination(string user, string game, string version=null, string[] channels=null) : this()
        {
            m_user = user;
            m_game = game;
            m_version = version ?? "";
            m_channels = channels != null ? new List<string>(channels) : new List<string>();
        }
        
        public void SetGame(string user, string game)
        {
            m_user = user;
            m_game = game;
        }
        
        public void SetChannels(params string[] channels)
        {
            m_channels = new List<string>(channels);
        }
        
        public void SetVersion(string version)
        {
            m_version = version;
        }

        public override async Task<bool> Upload(BuildTaskReport.StepResult result)
        {
            string filePath = StringFormatter.FormatString(m_filePath);

            string user = StringFormatter.FormatString(m_user);
            string game = StringFormatter.FormatString(m_game);
            string version = StringFormatter.FormatString(m_version);
            List<string> channels = m_channels.ConvertAll(StringFormatter.FormatString);
            
            int processID = ProgressUtils.Start("Itchio", "Uploading to Itchio");
            bool success = await Itchio.Instance.Upload(filePath, user, game, channels, version, result);
            ProgressUtils.Remove(processID);
            
            return success;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);
            
            if (!InternalUtils.GetService<ItchioService>().IsReadyToStartBuild(out string serviceReason))
            {
                errors.Add(serviceReason);
            }
            
            if (string.IsNullOrEmpty(m_user))
            {
                errors.Add("User not specified");
            }

            if (string.IsNullOrEmpty(m_game))
            {
                errors.Add("Game not specified");
            }
            
            if (m_channels.Count == 0)
            {
                errors.Add("No channels specified");
            }
            
            if (string.IsNullOrEmpty(m_version))
            {
                errors.Add("Version not specified");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                ["user"] = m_user,
                ["game"] = m_game,
                ["version"] = m_version,
                ["channels"] = m_channels
            };
            return dict;
        }

        public override void Deserialize(Dictionary<string, object> s)
        {
            if (s.TryGetValue("user", out object user))
            {
                m_user = (string)user;
            }
            if (s.TryGetValue("game", out object game))
            {
                m_game = (string)game;
            }
            if (s.TryGetValue("version", out object version))
            {
                m_version = (string)version;
            }

            if (s.TryGetValue("channels", out object channels))
            {
                m_channels = ((List<object>)channels).ConvertAll(a => (string)a);
            }
        }
    }
}