namespace Wireframe
{
    public readonly struct AppleAltoolUploadResponse
    {
        public readonly bool Successful;

        public AppleAltoolUploadResponse(bool successful)
        {
            Successful = successful;
        }
    }

    public readonly struct AppleFindBuildResponse
    {
        public readonly bool Successful;

        /// <summary>The App Store Connect "build" resource ID.</summary>
        public readonly string BuildId;

        public AppleFindBuildResponse(bool successful, string buildId = "")
        {
            Successful = successful;
            BuildId = buildId;
        }
    }

    public readonly struct AppleSimpleResponse
    {
        public readonly bool Successful;

        public AppleSimpleResponse(bool successful)
        {
            Successful = successful;
        }
    }
}
