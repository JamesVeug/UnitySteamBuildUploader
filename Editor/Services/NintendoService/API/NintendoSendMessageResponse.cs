namespace Wireframe
{
    public readonly struct NintendoSendMessageResponse
    {
        public readonly bool Successful;
        public readonly string MessageId;

        public NintendoSendMessageResponse(bool successful, string messageId = "")
        {
            Successful = successful;
            MessageId = messageId;
        }
    }
}
