namespace Wireframe
{
    public readonly struct DropboxUploadResponse
    {
        public readonly bool Successful;
        public readonly string PathDisplay;
        public readonly string FileId;

        public DropboxUploadResponse(bool successful, string pathDisplay = "", string fileId = "")
        {
            Successful = successful;
            PathDisplay = pathDisplay;
            FileId = fileId;
        }
    }

    public readonly struct DropboxSharedLinkResponse
    {
        public readonly bool Successful;
        public readonly string Url;

        public DropboxSharedLinkResponse(bool successful, string url = "")
        {
            Successful = successful;
            Url = url;
        }
    }
}
