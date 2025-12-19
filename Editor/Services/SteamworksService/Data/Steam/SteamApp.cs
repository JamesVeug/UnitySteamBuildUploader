using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class SteamApp : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID;
        public string Name = "Template";
        public AppVDFFile App = new AppVDFFile();
        public List<SteamDepot> Depots = new List<SteamDepot>();
        public List<SteamBranch> ConfigBranches = new List<SteamBranch>();

        // Deprecated in v1.1.3 in favor of ConfigBranches so we can add more fields for a branch
        [Obsolete("Use ConfigBranches instead")]
        public List<string> Branches;
        
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

        public SteamApp(AppVDFFile appFile)
        {
            App = new AppVDFFile(appFile);
            
            ConfigBranches.Add(new SteamBranch(1, "none"));
            
            if (!string.IsNullOrEmpty(appFile.setlive) && !appFile.setlive.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                ConfigBranches.Add(new SteamBranch(2, appFile.setlive));
            }
            
            foreach (VdfMap<int, string>.MapData depotData in appFile.depots.GetData())
            {
                DepotVDFFile depotVDF = VDFFile.Load<DepotVDFFile>(depotData.Value);
                if (depotVDF == null)
                {
                    continue;
                }
                
                SteamDepot depot = new SteamDepot(Depots.Count + 1, depotVDF.FileName);
                depot.Depot = depotVDF;
                Depots.Add(depot);
            }
        }
    }

    [Serializable]
    public class SteamDepot : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;
        
        public int ID;
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