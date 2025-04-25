using System;

namespace Wireframe
{
    public class BuildModifierAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public BuildModifierAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}