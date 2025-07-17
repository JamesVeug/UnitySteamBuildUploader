using System.Collections.Generic;

namespace Wireframe
{
    internal static partial class ItchioUIUtils
    {
        public class ItchioGamePopup : CustomMultiDropdown<ItchioUser, ItchioGameData>
        {
            public override string FirstEntryText => "Choose Game";
            
            public override List<(ItchioUser, List<ItchioGameData>)> GetAllData()
            {
                GetItchioBuildData();

                return data.UserToGames();
            }
        }
    }
}