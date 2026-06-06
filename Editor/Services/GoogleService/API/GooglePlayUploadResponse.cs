namespace Wireframe
{
    /// <summary>
    /// Outcome of an end-to-end Google Play upload (create edit → upload binary → assign to track → commit edit)
    /// </summary>
    public readonly struct GooglePlayUploadResponse
    {
        public readonly bool Successful;
        public readonly long VersionCode;
        public readonly string PackageName;
        public readonly string Track;
        public readonly string EditId;

        public GooglePlayUploadResponse(bool successful, long versionCode = 0, string packageName = "", string track = "", string editId = "")
        {
            Successful = successful;
            VersionCode = versionCode;
            PackageName = packageName;
            Track = track;
            EditId = editId;
        }
    }
}
