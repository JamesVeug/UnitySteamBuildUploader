using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// A GUIContent for errors/warnings that the user can fix in Preferences or Project Settings.
    /// When drawn in the UI a button is shown next to the message that opens the relevant settings window
    /// so the user does not need to navigate there manually.
    /// </summary>
    public class SettingsLinkGUIContent : GUIContent
    {
        public string SettingsPath { get; }
        public SettingsScope Scope { get; }

        /// <summary>
        /// Set when the reason for this message is that the user has to log in. Lets the error draw an
        /// AuthStatusButton next to the settings button so it can be fixed without leaving the window.
        /// </summary>
        public IAuthenticatedService AuthService { get; }

        public string ButtonText => Scope == SettingsScope.User ? "Open Preferences" : "Open Project Settings";

        public SettingsLinkGUIContent(string text, string tooltip, string settingsPath, SettingsScope scope,
            IAuthenticatedService authService = null) : base(text, tooltip)
        {
            SettingsPath = settingsPath;
            Scope = scope;
            AuthService = authService;
        }

        public void OpenSettings()
        {
            if (Scope == SettingsScope.User)
            {
                SettingsService.OpenUserPreferences(SettingsPath);
            }
            else
            {
                SettingsService.OpenProjectSettings(SettingsPath);
            }
        }
    }
}
