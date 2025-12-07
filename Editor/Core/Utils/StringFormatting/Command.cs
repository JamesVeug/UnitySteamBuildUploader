using System;

namespace Wireframe
{
    public class Command : IComparable<Command>
    {
        public string Key;
        public string Tooltip;
        public Func<string> Formatter;
            
        public Command(string key, Func<string> formatter, string tooltip)
        {
            Key = key;
            Tooltip = tooltip;
            Formatter = formatter;
        }

        public int CompareTo(Command other)
        {
            return String.Compare(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }
    }
}