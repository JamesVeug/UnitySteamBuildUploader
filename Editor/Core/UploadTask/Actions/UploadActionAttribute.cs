using System;

namespace Wireframe
{
    /// <summary>
    /// Marks a class as it can be used as an upload action and creates one using reflection
    /// So your class will need a parameterless constructor
    /// </summary>
    public class UploadActionAttribute : Attribute
    {
        /// <summary>
        /// What to display in UI dropdowns for the user
        /// </summary>
        public string DisplayName { get; }
        
        public string ButtonText { get; }
        
        public UploadActionAttribute(string displayName, string buttonText)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
        }
    }
}