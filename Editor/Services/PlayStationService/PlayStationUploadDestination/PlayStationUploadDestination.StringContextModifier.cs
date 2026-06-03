namespace Wireframe
{
    public partial class PlayStationUploadDestination
    {
        public const string PLAYSTATION_TITLE_NAME_KEY = "{playstationTitleName}";
        public const string PLAYSTATION_BRANCH_NAME_KEY = "{playstationBranchName}";

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(PLAYSTATION_TITLE_NAME_KEY, GetTitleName, "The name of the PlayStation Title that is being uploaded to.");
            context.AddCommand(PLAYSTATION_BRANCH_NAME_KEY, GetBranchName, "The name of the PlayStation Branch that is being uploaded to.");
            return context;
        }

        private string GetBranchName()
        {
            if (m_uploadBranch != null)
            {
                return m_uploadBranch.DisplayName;
            }

            if (m_destinationBranch != null)
            {
                return m_destinationBranch.DisplayName;
            }

            return "Unspecified Branch";
        }

        private string GetTitleName()
        {
            if (m_uploadApp != null)
            {
                return m_uploadApp.DisplayName;
            }

            if (m_app != null)
            {
                return m_app.DisplayName;
            }

            return "Unspecified Title";
        }
    }
}
