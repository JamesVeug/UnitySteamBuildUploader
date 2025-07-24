using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Send a message to a channel on a discord server using a bot.
    /// 
    /// NOTE: This classes name path is saved in the JSON file so avoid renaming
    /// </summary>
    [Wiki(nameof(DiscordMessageChannelAction), "actions", "Use a bot to send a message to a channel on a discord server.")]
    [BuildAction("Discord Message Channel", "Discord Send Message to Channel")]
    public partial class DiscordMessageChannelAction : ABuildAction
    {
        [Wiki("App", "Which Steam App to upload to. eg: 1141030", 1)]
        private DiscordConfig.DiscordApp m_app;
        
        [Wiki("Server", "What server to send the message to", 2)]
        private DiscordConfig.DiscordServer m_server;
        
        [Wiki("Channel", "What channel in the server to send the message to", 2)]
        private DiscordConfig.DiscordChannel m_channel;
        
        [Wiki("Text", "What text to send", 3)]
        private string m_text;
        
        public override async Task<bool> Execute(BuildTaskReport.StepResult stepResult)
        {
            List<Dictionary<string, object>> embeds = new List<Dictionary<string, object>>();
            Dictionary<string, object> descriptionEmbed = new Dictionary<string, object>
            {
                // { "title", StringFormatter.FormatString("{version}") },
                { "description", m_buildDescription },
                { "color", m_successful ? 0x00FF00 : 0xFF0000 }
            };
            embeds.Add(descriptionEmbed);
            return await Discord.SendMessageToChannel(m_channel.ChannelID, m_text, m_app.Token, m_app.IsBot, embeds, stepResult);
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);

            if (!Discord.Enabled)
            {
                errors.Add("Discord is not enabled. Please enable it in the settings.");
            }
            
            if (m_app == null)
            {
                errors.Add("Discord App is not set. Please select a Discord App.");
            }
            else if (string.IsNullOrEmpty(m_app.Token))
            {
                errors.Add($"Discord App {m_app.Name} does not have a token set. Please set the token in the Preferences!");
            }
            
            if (m_server == null)
            {
                errors.Add("Server is not set. Please select a Discord Server.");
            }
            
            if (m_channel == null)
            {
                errors.Add("Channel is not set. Please set the Channel ID.");
            }
            
            if (string.IsNullOrEmpty(m_text))
            {
                errors.Add("Text is not set. Please set the text to send.");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>
            {
                { "app", m_app?.Id ?? 0 },
                { "serverId", m_server?.Id ?? 0 },
                { "channelId", m_channel?.ChannelID ?? 0 },
                { "text", m_text }
            };
            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            DiscordConfig.DiscordApp[] buildConfigs = DiscordUIUtils.AppPopup.Values;
            if (data.TryGetValue("app", out object configIDString) && configIDString != null)
            {
                m_app = buildConfigs.FirstOrDefault(a => a.Id == (long)configIDString);
            }
            
            DiscordConfig.DiscordServer[] servers = DiscordUIUtils.ServerPopup.Values;
            if (data.TryGetValue("serverId", out object serverIDString) && serverIDString != null)
            {
                m_server = servers.FirstOrDefault(a => a.Id == (long)serverIDString);
            
                List<(DiscordConfig.DiscordServer, List<DiscordConfig.DiscordChannel>)> channels = DiscordUIUtils.ChannelPopup.GetAllData();
                if (data.TryGetValue("channelId", out object channelIDString) && channelIDString != null)
                {
                    m_channel = channels.FirstOrDefault(a => a.Item1.Id == m_server.Id)
                        .Item2.FirstOrDefault(c => c.ChannelID == (long)channelIDString);
                }
            }
            
            
            if (data.TryGetValue("text", out object textObj) && textObj != null)
            {
                m_text = textObj.ToString();
            }
            else
            {
                m_text = string.Empty; // Default to empty string if not set
            }
        }
    }
}
