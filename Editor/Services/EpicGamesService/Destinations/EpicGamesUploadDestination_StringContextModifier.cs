using System.Linq;

namespace Wireframe
{
    public partial class EpicGamesUploadDestination
    {
        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.EPIC_GAMES_ORGANIZATION_NAME_KEY, GetOrganizationName);
            context.AddCommand(Context.EPIC_GAMES_PRODUCT_NAME_KEY, GetProductName);
            context.AddCommand(Context.EPIC_GAMES_ARTIFACT_NAME_KEY, GetArtifactName);
            return context;
        }

        private string GetOrganizationName()
        {
            return Organization != null ? Organization.DisplayName : "Unspecified Organization";
        }

        private string GetProductName()
        {
            return Product != null ? Product.DisplayName : "Unspecified Product";
        }

        private string GetArtifactName()
        {
            return Artifact != null ? Artifact.DisplayName : "Unspecified Artifact";
        }
    }
}