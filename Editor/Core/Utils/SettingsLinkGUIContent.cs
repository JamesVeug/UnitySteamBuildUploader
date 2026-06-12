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

        public string ButtonText => Scope == SettingsScope.User ? "Open Preferences" : "Open Project Settings";

        public SettingsLinkGUIContent(string text, string tooltip, string settingsPath, SettingsScope scope) : base(text, tooltip)
        {
            SettingsPath = settingsPath;
            Scope = scope;
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
