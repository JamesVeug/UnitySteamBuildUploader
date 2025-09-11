using System;

namespace Wireframe
{
    /// <summary>
    /// Marks a class as an upload modifier and provides a user friendly name for it
    /// Used for reflection to find all upload modifiers
    /// So you will need a parameterless constructor
    /// </summary>
    public class UploadModifierAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public UploadModifierAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}