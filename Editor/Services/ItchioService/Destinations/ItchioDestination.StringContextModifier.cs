using System.Linq;

namespace Wireframe
{
    public partial class ItchioDestination
    {
        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.ITCHIO_USER_NAME_KEY, GetUserName);
            context.AddCommand(Context.ITCHIO_GAME_NAME_KEY, GetGameName);
            context.AddCommand(Context.ITCHIO_CHANNEL_NAME_KEY, GetChannelName);
            return context;
        }

        private string GetUserName()
        {
            return m_user != null ? m_user.DisplayName : "Unspecified User";
        }

        private string GetGameName()
        {
            return m_game != null ? m_game.DisplayName : "Unspecified Game";
        }

        private string GetChannelName()
        {
            return m_channels != null && m_channels.Count > 0
                ? string.Join(",", m_channels.Select(a => a.DisplayName))
                : "Unspecified Channels";
        }
    }
}