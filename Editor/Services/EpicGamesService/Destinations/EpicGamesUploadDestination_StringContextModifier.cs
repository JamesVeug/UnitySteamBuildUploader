using System.Linq;

namespace Wireframe
{
    public partial class EpicGamesUploadDestination : StringFormatter.IContextModifier
    {
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            if (key == StringFormatter.EPIC_GAMES_ORGANIZATION_NAME_KEY)
            {
                if (Organization != null)
                {
                    value = Organization.DisplayName;
                }
                else
                {
                    value = "Unspecified Organization";
                }

                return true;
            }
            else if (key == StringFormatter.EPIC_GAMES_PRODUCT_NAME_KEY)
            {
                if (Product != null)
                {
                    value = Product.DisplayName;
                }
                else
                {
                    value = "Unspecified Product";
                }

                return true;
            }
            else if (key == StringFormatter.EPIC_GAMES_ARTIFACT_NAME_KEY)
            {
                if (Artifact != null)
                {
                    value = Artifact.DisplayName;
                }
                else
                {
                    value = "Unspecified Artifact";
                }

                return true;
            }
            
            value = "";
            return false;
        }
    }
}