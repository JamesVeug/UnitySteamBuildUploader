using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class PlayStationApp : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string Name = "Template";
        public string TitleID = "";
        public string ContentID = "";
        public string DefaultBranch = "none";
        public List<PlayStationBranch> ConfigBranches = new List<PlayStationBranch>();

        public PlayStationApp()
        {
            ConfigBranches.Add(new PlayStationBranch(1, "none"));
        }

        public PlayStationApp(PlayStationApp current)
        {
            Name = current.Name;
            TitleID = current.TitleID;
            ContentID = current.ContentID;
            DefaultBranch = current.DefaultBranch;

            ConfigBranches = new List<PlayStationBranch>(current.ConfigBranches);
        }
    }
}
