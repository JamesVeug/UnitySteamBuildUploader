using System;

namespace Wireframe
{
    public class UploadModifierAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public UploadModifierAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}