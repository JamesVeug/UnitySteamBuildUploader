namespace Wireframe
{
    public readonly struct GoogleChatSendMessageResponse
    {
        public readonly bool Successful;
        public readonly string MessageName; // Full resource name e.g. "spaces/AAA.../messages/BBB.CCC"

        public GoogleChatSendMessageResponse(bool successful, string messageName = "")
        {
            Successful = successful;
            MessageName = messageName;
        }
    }
}
