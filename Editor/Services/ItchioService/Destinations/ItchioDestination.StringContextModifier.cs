using System.Linq;

namespace Wireframe
{
    public partial class ItchioDestination : StringFormatter.IContextModifier
    {
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            if (key == StringFormatter.ITCHIO_USER_NAME_KEY)
            {
                if (m_user != null)
                {
                    value = m_user.DisplayName;
                }
                else
                {
                    value = "Unspecified User";
                }

                return true;
            }
            else if (key == StringFormatter.ITCHIO_GAME_NAME_KEY)
            {
                if (m_game != null)
                {
                    value = m_game.DisplayName;
                }
                else
                {
                    value = "Unspecified Game";
                }

                return true;
            }
            else if (key == StringFormatter.ITCHIO_CHANNEL_NAME_KEY)
            {
                if (m_channels != null && m_channels.Count > 0)
                {
                    value = string.Join(",", m_channels.Select(a=>a.DisplayName));
                }
                else
                {
                    value = "Unspecified Channels";
                }

                return true;
            }
            
            value = "";
            return false;
        }
    }
}