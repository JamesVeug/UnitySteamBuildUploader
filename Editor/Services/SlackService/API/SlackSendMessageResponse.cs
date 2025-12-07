namespace Wireframe
{
    public readonly struct SlackSendMessageResponse
    {
        public readonly bool Successful;
        public readonly string MessageTimeStamp; // Treat this as its ID according to the api docs
        
        public SlackSendMessageResponse(bool successful, string messageTimeStamp = "")
        {
            Successful = successful;
            MessageTimeStamp = messageTimeStamp;
        }
    }
}