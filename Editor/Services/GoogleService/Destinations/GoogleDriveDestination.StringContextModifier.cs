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
            context.AddCommand(Context.GOOGLE_DRIVE_FOLDER_FILE_ID_KEY, GetFileID);
            context.AddCommand(Context.GOOGLE_DRIVE_FOLDER_WEB_VIEW_URL_KEY, GetWebViewLink);
            return context;
        }

        private string GetAppName()
        {
            return m_app != null ? m_app.DisplayName : "Unspecified Google App";
        }

        private string GetFolderName()
        {
            return m_folder != null ? m_folder.DisplayName : "My Folder";
        }

        private string GetFileID()
        {
            return m_recordedFileId;
        }

        private string GetWebViewLink()
        {
            return m_recordedWebViewLink;
        }
    }
}
