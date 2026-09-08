using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public abstract class AService
    {
        public abstract string ServiceName { get; }
        public abstract string[] SearchKeywords { get; }
        public SettingsLinkGUIContent DisabledServiceGUI => PreferencesLink($"{ServiceName} is not enabled", $"All {ServiceName} services are disabled until enabled.");
        internal virtual WindowTab WindowTabType => null;
        public abstract bool IsReadyToStartBuild(out GUIContent reason);
        public abstract bool IsProjectSettingsSetup();

        public virtual string PreferencesPath => "Preferences/Build Uploader/Services/" + ServiceName;
        public virtual string ProjectSettingsPath => "Project/Build Uploader/Services/" + ServiceName;

        /// <param name="authService">Pass this when the reason is that the user needs to log in, so the
        /// message can show a button that logs them in.</param>
        public SettingsLinkGUIContent PreferencesLink(string text, string tooltip, IAuthenticatedService authService = null)
        {
            return new SettingsLinkGUIContent(text, tooltip, PreferencesPath, SettingsScope.User, authService);
        }

        public SettingsLinkGUIContent ProjectSettingsLink(string text, string tooltip)
        {
            return new SettingsLinkGUIContent(text, tooltip, ProjectSettingsPath, SettingsScope.Project);
        }

        public virtual void PreferencesGUI()
        {
            if (GetType().GetCustomAttribute(typeof(ExperimentalAttribute)) != null)
            {
                EditorGUILayout.HelpBox("This Service is Experimental and may contain bugs. Report any issues to: https://github.com/JamesVeug/UnityBuildUploader/issues", MessageType.Warning);
            }
        }

        public virtual void ProjectSettingsGUI()
        {
            if (GetType().GetCustomAttribute(typeof(ExperimentalAttribute)) != null)
            {
                EditorGUILayout.HelpBox("This Service is Experimental and may contain bugs. Report any issues to: https://github.com/JamesVeug/UnityBuildUploader/issues", MessageType.Warning);
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
