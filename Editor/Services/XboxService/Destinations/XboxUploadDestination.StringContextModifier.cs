namespace Wireframe
{
    public partial class XboxUploadDestination
    {
        private const string submissionIdTooltip =
            "After a successful upload the Microsoft Store submission ID is stored under this key " +
            "so later actions can reference it. Example: XboxSubmissionId  (no curly braces).";

        private Command m_submissionIdFormat;

        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            m_submissionIdFormat = context.AddCommand("", GetSubmissionId, submissionIdTooltip);
            return context;
        }

        private string GetSubmissionId()
        {
            return m_cachedSubmissionId;
        }
    }
}
