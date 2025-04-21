using System;

namespace Wireframe
{
    [Serializable]
    public class SteamBranch : DropdownElement
    {
        public int Id => id;
        public string DisplayName => name;

        public int id;
        public string name;

        public SteamBranch(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        
        public SteamBranch(SteamBranch branch)
        {
            id = branch.id;
            name = branch.name;
        }
    }
}