using System;

namespace Wireframe
{
    public class BuildDestinationAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public BuildDestinationAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}