namespace Wireframe
{
    public partial class NintendoNotifyAction
    {
        private const string idFormatTooltip = "When sending a Nintendo notification we may receive a Message ID. If a formatName is provided then that ID can be used elsewhere in the Upload Task. eg: NintendoNotifyId (NOTE: Do not include curly braces)";

        private string m_recordedResponseId;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            m_responseIdFormat = context.AddCommand("", GetResponseId, idFormatTooltip); // Key is replaced later
            return context;
        }

        private string GetResponseId()
        {
            return m_recordedResponseId;
        }
    }
}
