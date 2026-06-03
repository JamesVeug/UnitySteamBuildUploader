using System;

namespace Wireframe
{
    [Serializable]
    public class PlayStationBranch : DropdownElement
    {
        public int Id => id;
        public string DisplayName => name;

        public int id;
        public string name;

        public PlayStationBranch(string name)
        {
            this.id = -1;
            this.name = name;
        }

        public PlayStationBranch(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public PlayStationBranch(PlayStationBranch branch)
        {
            id = branch.id;
            name = branch.name;
        }
    }
}
