using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class ItchioUIUtils
    {
        public class ItchioUserPopup : CustomDropdown<ItchioUser>
        {
            public override string FirstEntryText => "Choose User";

            protected override List<ItchioUser> FetchAllData()
            {
                GetItchioBuildData();
                return data.Users;
            }
        }
    }
}