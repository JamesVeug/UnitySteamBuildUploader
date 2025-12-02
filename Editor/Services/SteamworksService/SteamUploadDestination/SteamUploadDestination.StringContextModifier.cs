using System.Linq;

namespace Wireframe
{
    public partial class SteamUploadDestination : StringFormatter.IContextModifier
    {
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            if (key == StringFormatter.STEAM_APP_NAME_KEY)
            {
                if (m_uploadApp != null)
                {
                    value = m_uploadApp.DisplayName;
                }
                else if (m_app != null)
                {
                    value = m_app.DisplayName;
                }
                else
                {
                    value = "Unspecified App";
                }

                return true;
            }
            else if (key == StringFormatter.STEAM_BRANCH_NAME_KEY)
            {
                if (m_uploadBranch != null)
                {
                    value = m_uploadBranch.DisplayName;
                }
                else if (m_destinationBranch != null)
                {
                    value = m_destinationBranch.DisplayName;
                }
                else
                {
                    value = "Unspecified Branch";
                }

                return true;
            }
            else if (key == StringFormatter.STEAM_DEPOT_NAME_KEY)
            {
                if (m_uploadDepots != null && m_uploadDepots.Count > 0)
                {
                    value = string.Join(",", m_uploadDepots.Select(a=>a.DisplayName));
                }
                else if(m_depots != null)
                {
                    value = string.Join(",", m_depots.Select(a=>a.DisplayName));
                }
                else
                {
                    value = "Unspecified Depots";
                }

                return true;
            }
            
            value = "";
            return false;
        }
    }
}