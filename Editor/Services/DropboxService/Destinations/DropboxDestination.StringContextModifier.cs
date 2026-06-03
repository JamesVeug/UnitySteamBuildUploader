namespace Wireframe
{
    public partial class DropboxDestination
    {
        // Populated by Upload() after each successful file upload.
        private string m_recordedPath;
        private string m_recordedShareLink;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.DROPBOX_APP_NAME_KEY, GetAppName);
            context.AddCommand(Context.DROPBOX_FOLDER_NAME_KEY, GetFolderName);
            context.AddCommand(Context.DROPBOX_SHARE_LINK_KEY, GetShareLink);
            return context;
        }

        private string GetAppName()
        {
            return m_app != null ? m_app.DisplayName : "Unspecified Dropbox App";
        }

        private string GetFolderName()
        {
            return m_folder != null ? m_folder.DisplayName : "Root";
        }

        private string GetShareLink()
        {
            return string.IsNullOrEmpty(m_recordedShareLink) ? "" : m_recordedShareLink;
        }
    }
}
