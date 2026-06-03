using System.Collections.Generic;

namespace Wireframe
{
    [System.Serializable]
    public class PlayStationAppData
    {
        public List<PlayStationApp> Configs = new List<PlayStationApp>();

        public List<(PlayStationApp, List<PlayStationBranch>)> ConfigToBranches()
        {
            var dataConfigToBranchOptionValues = new List<(PlayStationApp, List<PlayStationBranch>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                PlayStationApp config = Configs[i];
                dataConfigToBranchOptionValues.Add((config, config.ConfigBranches));
            }

            return dataConfigToBranchOptionValues;
        }
    }
}
