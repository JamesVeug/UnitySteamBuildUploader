namespace Wireframe
{
    public partial class GooglePlayDestination
    {
        // Populated by Upload() after a successful publish so later actions in the
        // pipeline can surface them via {googlePlay…} format keys.
        private long m_recordedVersionCode;
        private string m_recordedEditId;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.GOOGLE_PLAY_PACKAGE_NAME_KEY, GetPackageName);
            context.AddCommand(Context.GOOGLE_PLAY_TRACK_KEY, GetTrackName);
            context.AddCommand(Context.GOOGLE_PLAY_VERSION_CODE_KEY, GetVersionCode);
            context.AddCommand(Context.GOOGLE_PLAY_EDIT_ID_KEY, GetEditId);
            return context;
        }

        private string GetPackageName()
        {
            return m_playApp != null ? m_playApp.PackageName : "";
        }

        private string GetTrackName()
        {
            return GooglePlay.TrackName(m_track);
        }

        private string GetVersionCode()
        {
            return m_recordedVersionCode > 0 ? m_recordedVersionCode.ToString() : "";
        }

        private string GetEditId()
        {
            return m_recordedEditId ?? "";
        }
    }
}
