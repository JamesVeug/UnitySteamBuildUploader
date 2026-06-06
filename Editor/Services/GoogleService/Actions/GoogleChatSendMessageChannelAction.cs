using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    /// <summary>
    /// Send a message to a Google Chat space via a webhook.
    ///
    /// NOTE: This class's name path is saved in the JSON file so avoid renaming.
    /// </summary>
    [Wiki(nameof(GoogleChatSendMessageChannelAction), "actions", "Send a message to a Google Chat space using an incoming webhook.")]
    [UploadAction("Google Chat Send Message")]
    public partial class GoogleChatSendMessageChannelAction : AUploadAction
    {
        [Wiki("Space", "Which Google Chat space to send the message to. Webhook URL is configured in Preferences.", 1)]
        private GoogleConfig.GoogleChatSpace m_space;

        [Wiki("Text", "What text to send. Supports {keys} like {version}, {taskStatus} and {googleDriveFolderName}.", 2)]
        private string m_text = "";

        [Wiki("ResponseFormatName", "If a format key name is provided, the resource name of the sent message is stored under this key so later actions can reference it. eg: GoogleChatMessageName (no curly braces).", 3)]
        private Command m_responseMessageNameFormat; // Created in CreateContext()

        public GoogleChatSendMessageChannelAction() : base()
        {
            // Required for reflection
        }

        public void SetSpace(string spaceName)
        {
            m_space = new GoogleConfig.GoogleChatSpace { Name = spaceName };
        }

        public void SetText(string text)
        {
            m_text = text;
        }

        public void SetResponseMessageNameFormat(string formatName)
        {
            if (formatName.Length > 0)
            {
                if (formatName[0] != '{')
                {
                    formatName = '{' + formatName;
                }
                if (formatName[formatName.Length - 1] != '}')
                {
                    formatName += '}';
                }
            }

            m_responseMessageNameFormat.Key = formatName;
        }

        public override async Task<bool> Execute(UploadTaskReport.StepResult stepResult)
        {
            string text = m_context.FormatString(m_text);
            string webhookUrl = m_space.WebhookURL;
            GoogleChatSendMessageResponse response = await GoogleChat.SendMessage(text, webhookUrl, stepResult);
            m_recordedMessageName = response.MessageName;
            return response.Successful;
        }

        public override void TryGetErrors(List<string> errors)
        {
            base.TryGetErrors(errors);

            if (!Google.Enabled)
            {
                errors.Add("Google is not enabled. Enable it in the settings.");
            }

            if (m_space == null)
            {
                errors.Add("Google Chat Space is not set. Select a Space.");
            }
            else if (string.IsNullOrEmpty(m_space.WebhookURL))
            {
                errors.Add($"Google Chat Space {m_space.Name} does not have a Webhook URL set. Set it in Preferences.");
            }

            if (string.IsNullOrEmpty(m_text))
            {
                errors.Add("Text is not set. Set the text to send.");
            }
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "space", m_space?.Id ?? 0 },
                { "text", m_text },
                { "messageNameFormat", m_responseMessageNameFormat.Key }
            };
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            GoogleConfig.GoogleChatSpace[] spaces = GoogleUIUtils.ChatSpacePopup.Values;
            if (data.TryGetValue("space", out object spaceId) && spaceId != null)
            {
                m_space = spaces.FirstOrDefault(s => s.Id == (long)spaceId);
            }

            if (data.TryGetValue("text", out object textObj) && textObj != null)
            {
                m_text = textObj.ToString();
            }
            else
            {
                m_text = string.Empty;
            }

            if (data.TryGetValue("messageNameFormat", out object formatObj) && formatObj is string formatString)
            {
                m_responseMessageNameFormat.Key = formatString;
            }
        }
    }
}
