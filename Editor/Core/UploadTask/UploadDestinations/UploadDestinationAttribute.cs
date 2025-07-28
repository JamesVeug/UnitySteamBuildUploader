using System;

namespace Wireframe
{
    public class UploadDestinationAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public UploadDestinationAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}