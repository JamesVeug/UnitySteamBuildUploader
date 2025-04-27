using System;

namespace Wireframe
{
    public class WikiAttribute : Attribute
    {
        public string SubPath { get; }
        public string Name { get; }
        public string Text { get; }
        public WikiAttribute(string name, string subpath, string text)
        {
            SubPath = subpath;
            Name = name;
            Text = text;
        }
        
        public WikiAttribute(string name, string text)
        {
            Name = name;
            Text = text;
            SubPath = "";
        }
    }

    internal static class WikiAttributeExtensions
    {
        public static bool TryGetSourceWikiLink(this object source, out string url)
        {
            if (source == null)
            {
                url = null;
                return false;
            }
            
            return TryGetSourceWikiLink(source.GetType(), out url);
        }
        
        public static bool TryGetSourceWikiLink(this Type type, out string url)
        {
            var wikiAttribute = (WikiAttribute)Attribute.GetCustomAttribute(type, typeof(WikiAttribute));
            if (wikiAttribute == null)
            {
                url = null;
                return false;
            }

            url = $"https://github.com/JamesVeug/UnitySteamBuildUploader/wiki/{wikiAttribute.SubPath}#{wikiAttribute.Name}";
            return true;
        }
    }
}