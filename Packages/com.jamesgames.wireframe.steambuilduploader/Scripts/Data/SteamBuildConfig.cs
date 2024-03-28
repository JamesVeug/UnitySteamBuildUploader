using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class SteamBuildConfig
    {
        public string Name = "Template";
        public AppVDFFile App = new AppVDFFile();
        public List<SteamBuildDepot> Depots = new List<SteamBuildDepot>();
        public List<string> Branches = new List<string>();

        public SteamBuildConfig()
        {
            Branches.Add("none");
        }
    }

    [Serializable]
    public class SteamBuildDepot
    {
        public string Name = "Template";
        public DepotVDFFile Depot = new DepotVDFFile();
    }
}