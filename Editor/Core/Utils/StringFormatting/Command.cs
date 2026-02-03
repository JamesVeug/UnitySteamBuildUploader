using System;

namespace Wireframe
{
    public class Command : IComparable<Command>
    {
        public string Key;
        public string Tooltip;
        public Func<string> Formatter;
        public bool CanBeCached;
            
        public Command(string key, Func<string> formatter, string tooltip, bool canBeCached)
        {
            Key = key;
            Tooltip = tooltip;
            Formatter = formatter;
            CanBeCached = canBeCached;
        }

        public int CompareTo(Command other)
        {
            return String.Compare(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }
    }
}