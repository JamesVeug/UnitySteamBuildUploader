using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class SlackService
    {
        private static ReorderableListOfSlackAppsPreferences _reorderableListOfSlackAppsPreferences;

        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Apps are created on the developer dashboard.");
                if (GUILayout.Button("Developer Dashboard", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://api.slack.com/apps/");
                }
                if (GUILayout.Button("Documentation", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://docs.slack.dev/apis/web-api/");
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Slack.Enabled = GUILayout.Toggle(Slack.Enabled, "Enabled");
                if (!Slack.Enabled)
                {
                    return;
                }

                SlackConfig SlackConfig = SlackUIUtils.GetConfig();
                if (_reorderableListOfSlackAppsPreferences == null)
                {
                    _reorderableListOfSlackAppsPreferences = new ReorderableListOfSlackAppsPreferences();
                    _reorderableListOfSlackAppsPreferences.Initialize(SlackConfig.apps, "Apps", 
                        true, (_) => 
                        {
                            SlackUIUtils.AppPopup.Refresh();
                            SlackUIUtils.ServerPopup.Refresh();
                            SlackUIUtils.ChannelPopup.Refresh();
                            SlackUIUtils.Save();
                        });
                }

                if (_reorderableListOfSlackAppsPreferences.OnGUI())
                {
                    SlackUIUtils.AppPopup.Refresh();
                    SlackUIUtils.ServerPopup.Refresh();
                    SlackUIUtils.ChannelPopup.Refresh();
                    SlackUIUtils.Save();
                }
            }
        }
    }
}