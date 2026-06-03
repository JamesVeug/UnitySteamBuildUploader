namespace Wireframe
{
    public partial class GoogleDriveDestination
    {
        // Populated by Upload() after each successful file upload.
        private string m_recordedFileId;
        private string m_recordedWebViewLink;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.GOOGLE_DRIVE_APP_NAME_KEY, GetAppName);
            context.AddCommand(Context.GOOGLE_DRIVE_FOLDER_NAME_KEY, GetFolderName);
            return context;
        }

        private string GetAppName()
        {
            return m_app != null ? m_app.DisplayName : "Unspecified Google App";
        }

        private string GetFolderName()
        {
            return m_folder != null ? m_folder.DisplayName : "My Drive";
        }
    }
}
