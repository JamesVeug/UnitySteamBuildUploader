namespace Wireframe
{
    internal struct UploadResult
    {
        public bool Successful;
        public string FailReason;

        public UploadResult(bool successful, string errorText)
        {
            Successful = successful;
            FailReason = errorText;
        }

        public static UploadResult Success()
        {
            return new UploadResult
            {
                Successful = true
            };
        }

        public static UploadResult Failed(string reason)
        {
            return new UploadResult
            {
                Successful = false,
                FailReason = reason
            };
        }
    }
}