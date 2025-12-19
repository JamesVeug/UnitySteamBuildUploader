using System;
using System.Linq;

namespace Wireframe
{
    public abstract class AService
    {
        public abstract string ServiceName { get; }
        public abstract string[] SearchKeywords { get; }
        internal virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out string reason);
        public abstract bool IsProjectSettingsSetup();
        public abstract void PreferencesGUI();
        public abstract void ProjectSettingsGUI();
        public virtual bool HasProjectSettingsGUI => false;
        
        public bool MatchesSearchKeywords(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            bool matchesSearchKeywords = SearchKeywords.Any(a => Utils.Contains(a, search, StringComparison.OrdinalIgnoreCase));
            return matchesSearchKeywords;
        }
    }
}
