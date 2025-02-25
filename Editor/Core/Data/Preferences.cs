using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    internal static class Preferences
    {
        [PreferenceItem("Build Uploader")]
        private static void OnPreferencesGUI()
        {
            GUILayout.Label("Preferences for the Build Uploader. Required to log into various services.",
                EditorStyles.wordWrappedLabel);

            foreach (AService service in InternalUtils.AllServices())
            {
                GUILayout.Space(20);
                service.PreferencesGUI();
            }

            GUILayout.Space(20);
        }
    }
}