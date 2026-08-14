using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class GitService
    {
        public override void PreferencesGUI()
        {
            base.PreferencesGUI();
            using (new EditorGUILayout.VerticalScope("box"))
            {
                Git.Enabled = GUILayout.Toggle(Git.Enabled, "Enabled");
                using (new EditorGUI.DisabledScope(!Git.Enabled))
                {
                    DrawGitPath();
                }
            }
        }

        private static void DrawGitPath()
        {
            bool found = Git.Enabled && Git.IsAvailable;
            using (new GUILayout.HorizontalScope())
            {
                Color temp = GUI.color;
                GUI.color = found ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("Git Path:",
                        "The path to the git executable. Leave empty to use the git on your PATH, which is what most machines want."),
                    GUILayout.Width(105));
                GUI.color = temp;

                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://git-scm.com/downloads");
                }

                string newPath = GUILayout.TextField(Git.ExecutablePath);

                if (GUILayout.Button("...", GUILayout.Width(50)))
                {
                    string pickedPath = EditorUtility.OpenFilePanel("Git Executable", ".", "");
                    if (!string.IsNullOrEmpty(pickedPath))
                    {
                        newPath = pickedPath;
                    }
                }

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    Git.ResetExecutable();
                }

                Git.ExecutablePath = newPath;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(109);
                if (!Git.Enabled)
                {
                    GUILayout.Label("Enable the service to read the repository.", EditorStyles.miniLabel);
                }
                else if (found)
                {
                    GUILayout.Label($"Using {Git.Executable} - {Git.Version}", EditorStyles.miniLabel);
                }
                else if (string.IsNullOrEmpty(Git.ExecutablePath))
                {
                    GUILayout.Label("No git found on your PATH. Install git or point this at the executable.", EditorStyles.miniLabel);
                }
                else
                {
                    GUILayout.Label("That path did not run. Check it points at the git executable.", EditorStyles.miniLabel);
                }
            }
        }
    }
}
