using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Wireframe
{
    public abstract class AService
    {
        public abstract string ServiceName { get; }
        public abstract string[] SearchKeywords { get; }
        internal virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out string reason);
        public abstract bool IsProjectSettingsSetup();

        public virtual void PreferencesGUI()
        {
            if (GetType().GetCustomAttribute(typeof(ExperimentalAttribute)) != null)
            {
                EditorGUILayout.HelpBox("This Service is Experimental and may contain bugs. Report any issues to: https://github.com/JamesVeug/UnitySteamBuildUploader/issues", MessageType.Warning);
            }
        }

        public virtual void ProjectSettingsGUI()
        {
            if (GetType().GetCustomAttribute(typeof(ExperimentalAttribute)) != null)
            {
                EditorGUILayout.HelpBox("This Service is Experimental and may contain bugs. Report any issues to: https://github.com/JamesVeug/UnitySteamBuildUploader/issues", MessageType.Warning);
            }
        }

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
