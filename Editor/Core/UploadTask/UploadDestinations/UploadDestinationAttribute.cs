using System;

namespace Wireframe
{
    /// <summary>
    /// Marks a class as an upload destination and provides a user friendly name for it
    /// Used for reflection to find all upload destinations
    /// So you will need a parameterless constructor
    /// </summary>
    public class UploadDestinationAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public UploadDestinationAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}