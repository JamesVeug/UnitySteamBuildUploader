namespace Wireframe
{
    public readonly struct GoogleDriveUploadResponse
    {
        public readonly bool Successful;
        public readonly string FileId;
        public readonly string WebViewLink;

        public GoogleDriveUploadResponse(bool successful, string fileId = "", string webViewLink = "")
        {
            Successful = successful;
            FileId = fileId;
            WebViewLink = webViewLink;
        }
    }
}
