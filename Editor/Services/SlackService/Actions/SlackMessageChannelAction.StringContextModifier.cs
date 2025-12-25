namespace Wireframe
{
    public partial class SlackMessageChannelAction
    {
        private const string tsFormatTooltip = "When sending a Slack message we receive a Timestamp of that message. If a formatName is provided then that TS can be used elsewhere in the Upload Task. eg: editing it as a post action. eg: SlackMessageID (NOTE: Do not include curly braces)";
        
        private string m_recordedMessageTimeStamp;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            m_responseTSFormat = context.AddCommand("", getResponseTS, tsFormatTooltip); // Key is replaced later
            return context;
        }

        private string getResponseTS()
        {
            return m_recordedMessageTimeStamp;
        }
    }
}