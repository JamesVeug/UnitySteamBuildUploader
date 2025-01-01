using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class SteamBuildConfig : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID = 0;
        public string Name = "Template";
        public AppVDFFile App = new AppVDFFile();
        public List<SteamBuildDepot> Depots = new List<SteamBuildDepot>();
        public List<string> Branches = new List<string>();

        public SteamBuildConfig()
        {
            Branches.Add("none");
        }

        public SteamBuildConfig(SteamBuildConfig currentConfig)
        {
            Name = currentConfig.Name;
            App = new AppVDFFile(currentConfig.App);
            
            Depots = new List<SteamBuildDepot>(currentConfig.Depots);
            Branches = new List<string>(currentConfig.Branches);
        }
    }

    [Serializable]
    public class SteamBuildDepot
    {
        public string Name = "Template";
        public DepotVDFFile Depot = new DepotVDFFile();
        
        public SteamBuildDepot()
        {
        }
        
        public SteamBuildDepot(SteamBuildDepot currentDepot)
        {
            Name = currentDepot.Name;
            Depot = new DepotVDFFile(currentDepot.Depot);
        }
    }
}