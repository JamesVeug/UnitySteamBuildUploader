using System;

namespace Wireframe
{
    [Serializable]
    public class SteamBuildBranch : DropdownElement
    {
        public int Id => id;
        public string DisplayName => name;

        public int id;
        public string name;

        public SteamBuildBranch(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        
        public SteamBuildBranch(SteamBuildBranch branch)
        {
            id = branch.id;
            name = branch.name;
        }
    }
}