namespace Wireframe
{
    public partial class NintendoUploadDestination
    {
        public const string NINTENDO_TITLE_NAME_KEY = "{nintendoTitleName}";
        public const string NINTENDO_BRANCH_NAME_KEY = "{nintendoBranchName}";

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(NINTENDO_TITLE_NAME_KEY, GetTitleName, "The name of the Nintendo Title that is being uploaded to.");
            context.AddCommand(NINTENDO_BRANCH_NAME_KEY, GetBranchName, "The name of the Nintendo Branch that is being uploaded to.");
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
