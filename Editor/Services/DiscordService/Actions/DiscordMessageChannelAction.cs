using System;
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
    [UploadAction("Discord Message Channel")]
    public partial class DiscordMessageChannelAction : AUploadAction
    {
        [Serializable]
        [Wiki("Embed", "An embed to send with the message. This is optional and can be used to format the message nicely.")]
        public class Embed
        {
            [Wiki("Title", "The title of the embed. This is optional.")]
            public string title;
            
            [Wiki("Description", "The description of the embed. This is optional.")]
            public string description;
            
            [Wiki("Color", "The color of the embed in hexidecimal. This is optional.")]
            public string color;
        }
        
        [Wiki("App", "Which Steam App to upload to. eg: 1141030", 1)]
        private DiscordConfig.DiscordApp m_app;
        
        [Wiki("Server", "What server to send the message to", 2)]
        private DiscordConfig.DiscordServer m_server;
        
        [Wiki("Channel", "What channel in the server to send the message to", 2)]
        private DiscordConfig.DiscordChannel m_channel;

        [Wiki("Text", "What text to send", 3)] private string m_text = "";

        [Wiki("Embeds", "A list of embeds to send with the message. This is optional and can be used to format the message nicely.", 5)]
        private List<Embed> m_embeds = new List<Embed>();
        
        public override async Task<bool> Execute(UploadTaskReport.StepResult stepResult, StringFormatter.Context ctx)
        {
            List<Dictionary<string, object>> embeds = new List<Dictionary<string, object>>();
            foreach (Embed embed in m_embeds)
            {
                Dictionary<string, object> embedDict = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(embed.title))
                {
                    embedDict.Add("title", StringFormatter.FormatString(embed.title, ctx));
                }
                
                if (!string.IsNullOrEmpty(embed.description))
                {
                    embedDict.Add("description", StringFormatter.FormatString(embed.description, ctx));
                }
                
                if (!string.IsNullOrEmpty(embed.color))
                {
                    string colorHex = embed.color;
                    if(colorHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        colorHex = colorHex.Substring(2);
                    }
                    if(colorHex.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        colorHex = colorHex.Substring(1);
                    }
                    
                    embedDict.Add("color", int.Parse(colorHex, System.Globalization.NumberStyles.HexNumber));
                }
                
                embeds.Add(embedDict);
            }
            
            string text = StringFormatter.FormatString(m_text, ctx);
            return await Discord.SendMessageToChannel(m_channel.ChannelID, text, m_app.Token, m_app.IsBot, embeds, stepResult);
        }

        public override void TryGetErrors(List<string> errors, StringFormatter.Context ctx)
        {
            base.TryGetErrors(errors, ctx);

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
            
            if (m_embeds != null && m_embeds.Count > 0)
            {
                var embedsList = new List<object>();
                foreach (var embed in m_embeds)
                {
                    var embedDict = new Dictionary<string, object>
                    {
                        { "title", embed.title },
                        { "description", embed.description },
                        { "color", embed.color }
                    };
                    embedsList.Add(embedDict);
                }
                data["embeds"] = embedsList;
            }
            else
            {
                data["embeds"] = new List<object>(); // Ensure embeds is always present
            }
            
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
            
            if (data.TryGetValue("embeds", out object embedsObj) && embedsObj is List<object> embedsList)
            {
                m_embeds = new List<Embed>();
                foreach (var embedData in embedsList)
                {
                    if (embedData is Dictionary<string, object> embedDict)
                    {
                        Embed embed = new Embed
                        {
                            title = embedDict.ContainsKey("title") ? embedDict["title"].ToString() : string.Empty,
                            description = embedDict.ContainsKey("description") ? embedDict["description"].ToString() : string.Empty,
                            color = embedDict.ContainsKey("color") ? embedDict["color"].ToString() : string.Empty
                        };
                        m_embeds.Add(embed);
                    }
                }
            }
            else
            {
                m_embeds = new List<Embed>(); // Default to empty list if not set
            }
        }
    }
}
