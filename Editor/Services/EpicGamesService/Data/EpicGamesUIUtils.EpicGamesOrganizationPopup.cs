using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class EpicGamesUIUtils
    {
        public class EpicGamesOrganizationPopup : CustomDropdown<EpicGamesOrganization>
        {
            public override string FirstEntryText => "Choose Organization";

            protected override List<EpicGamesOrganization> FetchAllData()
            {
                GetEpicGamesData();
                return data.Organizations;
            }
        }
    }
}