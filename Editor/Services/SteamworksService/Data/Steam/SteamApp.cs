using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Wireframe
{
    [Serializable]
    public class SteamApp : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID = 0;
        public string Name = "Template";
        public AppVDFFile App = new AppVDFFile();
        public List<SteamDepot> Depots = new List<SteamDepot>();
        public List<SteamBranch> ConfigBranches = new List<SteamBranch>();

        // Deprecated in v1.1.3 in favor of ConfigBranches so we can add more fields for a branch
        [Obsolete("Use ConfigBranches instead")]
        public List<string> Branches = null;
        
        public SteamApp()
        {
            ConfigBranches.Add(new SteamBranch(1, "none"));
        }

        public SteamApp(SteamApp current)
        {
            Name = current.Name;
            App = new AppVDFFile(current.App);
            
            Depots = new List<SteamDepot>(current.Depots);
            ConfigBranches = new List<SteamBranch>(current.ConfigBranches);
        }
    }

    [Serializable]
    public class SteamDepot : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID = 0;
        public string Name = "Template";
        public DepotVDFFile Depot = new DepotVDFFile();
        
        public SteamDepot()
        {
            
        }
        
        public SteamDepot(int id, string name)
        {
            ID = id;
            Name = name;
        }
        
        public SteamDepot(SteamDepot currentDepot)
        {
            Name = currentDepot.Name;
            Depot = new DepotVDFFile(currentDepot.Depot);
        }

    }
}