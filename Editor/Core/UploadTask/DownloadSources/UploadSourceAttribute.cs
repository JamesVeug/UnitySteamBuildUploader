using System;

namespace Wireframe
{
    public class UploadSourceAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        
        public UploadSourceAttribute(string displayName, string buttonText)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
        }
    }
}