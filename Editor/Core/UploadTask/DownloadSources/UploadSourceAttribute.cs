using System;

namespace Wireframe
{
    public class UploadSourceAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        public bool CanCacheContents { get; }
        
        /// <summary>
        /// Define how this attribute displays and acts for all instances
        /// </summary>
        /// <param name="displayName">What should be displayed in the UI</param>
        /// <param name="buttonText">What text is shown in dropdowns to choose this source</param>
        /// <param name="canCacheContents">Whether this source can cache its contents in an isolated folder to be referenced between tasks.</param>
        public UploadSourceAttribute(string displayName, string buttonText, bool canCacheContents)
        {
            DisplayName = displayName;
            ButtonText = buttonText;
            CanCacheContents = canCacheContents;
        }
    }
}