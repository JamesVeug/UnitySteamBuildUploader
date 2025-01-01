using System;
using System.Collections.Generic;
using UnityEngine.UI;

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
        public List<SteamBuildBranch> ConfigBranches = new List<SteamBuildBranch>();

        // Deprecated in v1.1.3 in favor of ConfigBranches so we can add more fields for a branch
        [Obsolete("Use ConfigBranches instead")]
        public List<string> Branches = null;
        
        public SteamBuildConfig()
        {
            ConfigBranches.Add(new SteamBuildBranch(1, "none"));
        }

        public SteamBuildConfig(SteamBuildConfig currentConfig)
        {
            Name = currentConfig.Name;
            App = new AppVDFFile(currentConfig.App);
            
            Depots = new List<SteamBuildDepot>(currentConfig.Depots);
            ConfigBranches = new List<SteamBuildBranch>(currentConfig.ConfigBranches);
        }
    }

    [Serializable]
    public class SteamBuildDepot : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID = 0;
        public string Name = "Template";
        public DepotVDFFile Depot = new DepotVDFFile();
        
        public SteamBuildDepot(int id, string name)
        {
            ID = id;
            Name = name;
        }
        
        public SteamBuildDepot(SteamBuildDepot currentDepot)
        {
            Name = currentDepot.Name;
            Depot = new DepotVDFFile(currentDepot.Depot);
        }

    }
}