using System.Collections.Generic;

namespace Wireframe
{
    [System.Serializable]
    public class NintendoAppData
    {
        public List<NintendoApp> Configs = new List<NintendoApp>();

        public List<(NintendoApp, List<NintendoBranch>)> ConfigToBranches()
        {
            var dataConfigToBranchOptionValues = new List<(NintendoApp, List<NintendoBranch>)>();
            for (int i = 0; i < Configs.Count; i++)
            {
                NintendoApp config = Configs[i];
                dataConfigToBranchOptionValues.Add((config, config.ConfigBranches));
            }

            return dataConfigToBranchOptionValues;
        }
    }
}
