using System;

namespace Wireframe
{
    [Serializable]
    public class DepotList : VdfMap<int, string>
    {
        public DepotList()
        {
            
        }
        
        public DepotList(DepotList depots)
        {
            foreach (var depot in depots.GetData())
            {
                Add(depot.Key, depot.Value);
            }
        }
    }
}