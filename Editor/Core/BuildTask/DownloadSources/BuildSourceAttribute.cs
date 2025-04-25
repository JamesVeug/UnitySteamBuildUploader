using System;

namespace Wireframe
{
    public class BuildSourceAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        
        public BuildSourceAttribute(string displayName, string buttonText)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
        }

    }
}