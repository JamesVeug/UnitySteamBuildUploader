using System;

namespace Wireframe
{
    public class UploadActionAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        
        public UploadActionAttribute(string displayName, string buttonText)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
        }
    }
}