using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class EpicGamesUIUtils
    {
        public class EpicGamesAllGamesPopup : CustomDropdown<EpicGamesProduct>
        {
            public override string FirstEntryText => "Choose Game";

            protected override List<EpicGamesProduct> FetchAllData()
            {
                GetEpicGamesData();
                List<EpicGamesProduct> result = new List<EpicGamesProduct>();
                foreach (var organization in data.Organizations)
                {
                    result.AddRange(organization.Products);
                }
                return result;
            }
        }
    }
}