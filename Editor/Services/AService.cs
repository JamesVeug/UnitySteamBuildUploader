using System;
using System.Linq;

namespace Wireframe
{
    public abstract class AService
    {
        public abstract string ServiceName { get; }
        public abstract string[] SearchKeyworks { get; }
        internal virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out string reason);
        public abstract void PreferencesGUI();
        public abstract void ProjectSettingsGUI();
        public virtual bool HasProjectSettingsGUI => false;
        
        public bool MatchesSearchKeywords(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            bool matchesSearchKeywords = SearchKeyworks.Any(a => a.Contains(search, StringComparison.OrdinalIgnoreCase));
            return matchesSearchKeywords;
        }
    }
}
