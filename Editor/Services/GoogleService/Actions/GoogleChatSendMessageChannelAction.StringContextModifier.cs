namespace Wireframe
{
    public partial class GoogleChatSendMessageChannelAction
    {
        private const string messageNameFormatTooltip =
            "When sending a Google Chat message the API returns the resource name of the created message " +
            "(e.g. 'spaces/AAAA.../messages/BBBB.CCCC'). If a format key name is provided then that resource name " +
            "is stored under that key so it can be referenced elsewhere in the Upload Task. " +
            "eg: GoogleChatMessageName (NOTE: Do not include curly braces)";

        private string m_recordedMessageName;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.GOOGLE_CHAT_SPACE_NAME_KEY, GetSpaceName);
            m_responseMessageNameFormat = context.AddCommand("", GetResponseMessageName, messageNameFormatTooltip); // Key replaced later
            return context;
        }

        private string GetSpaceName()
        {
            return m_space != null ? m_space.DisplayName : "Unspecified Space";
        }

        private string GetResponseMessageName()
        {
            return m_recordedMessageName;
        }
    }
}
