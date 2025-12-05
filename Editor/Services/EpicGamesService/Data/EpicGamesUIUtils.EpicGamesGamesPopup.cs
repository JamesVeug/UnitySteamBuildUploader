using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class EpicGamesUIUtils
    {
        public class EpicGamesGamesPopup : CustomMultiDropdown<EpicGamesOrganization, EpicGamesProduct>
        {
            public override string FirstEntryText => "Choose Game";
            public override bool AddChooseFromDropdownEntry => false;
            
            public override List<(EpicGamesOrganization, List<EpicGamesProduct>)> GetAllData()
            {
                GetEpicGamesData();
                List<(EpicGamesOrganization, List<EpicGamesProduct>)> result = new List<(EpicGamesOrganization, List<EpicGamesProduct>)>();
                foreach (EpicGamesOrganization organization in data.Organizations)
                {
                    List<EpicGamesProduct> games = new List<EpicGamesProduct>(organization.Products);
                    result.Add((organization, games));
                }
                return result;
            }
        }
    }
}