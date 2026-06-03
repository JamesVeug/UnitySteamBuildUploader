using System;

namespace Wireframe
{
    [Serializable]
    public class NintendoBranch : DropdownElement
    {
        public int Id => id;
        public string DisplayName => name;

        public int id;
        public string name;

        public NintendoBranch(string name)
        {
            this.id = -1;
            this.name = name;
        }

        public NintendoBranch(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public NintendoBranch(NintendoBranch branch)
        {
            id = branch.id;
            name = branch.name;
        }
    }
}
