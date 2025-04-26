using System;
using System.Reflection;

namespace Wireframe
{
    public class BuildSourceAttribute : Attribute
    {
        public string DisplayName { get; }
        public string ButtonText { get; }
        public string WikiLink { get; }
        
        public BuildSourceAttribute(string displayName, string buttonText, string urlSource=null)
        {
            DisplayName = displayName;
            ButtonText = buttonText;

            if (!string.IsNullOrEmpty(urlSource))
            {
                WikiLink = "https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/sources#" + urlSource;
            }
        }
    }
    
    internal static class BuildSourceAttributeExtensions
    {   
        public static string GetSourceWikiLink(this Type type)
        {
            var attribute = type?.GetCustomAttribute<BuildSourceAttribute>();
            return attribute?.WikiLink;
        }
    }
}