using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    public class NintendoApp : DropdownElement
    {
        public int Id => ID;
        public string DisplayName => Name;

        public int ID;
        public string Name = "Template";
        public string TitleID = "";
        public string ApplicationID = "";
        public string DefaultBranch = "none";
        public List<NintendoBranch> ConfigBranches = new List<NintendoBranch>();

        public NintendoApp()
        {
            ConfigBranches.Add(new NintendoBranch(1, "none"));
        }

        public NintendoApp(NintendoApp current)
        {
            Name = current.Name;
            TitleID = current.TitleID;
            ApplicationID = current.ApplicationID;
            DefaultBranch = current.DefaultBranch;

            ConfigBranches = new List<NintendoBranch>(current.ConfigBranches);
        }
    }
}
