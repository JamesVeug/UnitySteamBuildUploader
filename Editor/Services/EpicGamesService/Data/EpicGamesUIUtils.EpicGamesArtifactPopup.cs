using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class EpicGamesUIUtils
    {
        public class EpicGamesArtifactPopup : CustomMultiDropdown<EpicGamesProduct, EpicGamesArtifact>
        {
            public override string FirstEntryText => "Choose Artifact";
            
            public override List<(EpicGamesProduct, List<EpicGamesArtifact>)> GetAllData()
            {
                GetEpicGamesData();
                List<(EpicGamesProduct, List<EpicGamesArtifact>)> result = new List<(EpicGamesProduct, List<EpicGamesArtifact>)>();
                foreach (EpicGamesOrganization organization in data.Organizations)
                {
                    foreach (EpicGamesProduct game in organization.Products)
                    {
                        List<EpicGamesArtifact> artifacts = new List<EpicGamesArtifact>(game.Artifacts);
                        result.Add((game, artifacts));
                    }
                }
                return result;
            }
        }
    }
}