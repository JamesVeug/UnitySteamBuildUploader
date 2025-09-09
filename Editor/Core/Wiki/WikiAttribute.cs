using System;

namespace Wireframe
{
    public class WikiAttribute : Attribute
    {
        public string SubPath { get; }
        public string Name { get; }
        public string Text { get; }
        public int Order { get; }
        public WikiAttribute(string name, string subpath, string text, int order=0)
        {
            SubPath = subpath;
            Name = name;
            Text = text;
            Order = order;
        }
        
        public WikiAttribute(string name, string text, int order=0)
        {
            Name = name;
            Text = text;
            Order = order;
            SubPath = "";
        }
    }

    public class WikiEnumAttribute : WikiAttribute
    {
        public bool ListEnumValues;
        public WikiEnumAttribute(string name, string subpath, string text, bool listEnumValues, int order = 0) : base(name, subpath, text, order)
        {
            ListEnumValues = listEnumValues;
        }

        public WikiEnumAttribute(string name, string text, bool listEnumValues, int order = 0) : base(name, text, order)
        {
            ListEnumValues = listEnumValues;
        }
    }

    internal static class WikiAttributeExtensions
    {
        public static bool TryGetWikiLink(this object source, out string url)
        {
            if (source == null)
            {
                url = null;
                return false;
            }
            
            return TryGetWikiLink(source.GetType(), out url);
        }
        
        public static bool TryGetWikiLink(this Type type, out string url)
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