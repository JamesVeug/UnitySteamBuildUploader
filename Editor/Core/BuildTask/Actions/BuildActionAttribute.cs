using System;

namespace Wireframe
{
    public class BuildActionAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        
        public BuildActionAttribute(string displayName, string buttonText)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
        }
    }
}