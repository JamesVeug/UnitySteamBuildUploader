using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EpicGamesService
    {
        private Context ctx = new Context();
        private bool showFormattedSDKPath = true;
        private bool showFormattedCloudDirectory = true;
        
        public override void PreferencesGUI()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool newEnabled = GUILayout.Toggle(EpicGames.Enabled, "Enabled");
                if (newEnabled != EpicGames.Enabled)
                {
                    EpicGames.Enabled = newEnabled;
                }

                using (new EditorGUI.DisabledScope(!EpicGames.Enabled))
                {
                    DrawPreferences();
                }
            }
        }

        private void DrawPreferences()
        {
            using (new EditorGUILayout.HorizontalScope())
            {   
                GUILayout.Label("Epic Games uses the Build Patch tool to upload builds to Epic Games Store.");

                GUILayout.ExpandWidth(true);
                if (GUILayout.Button("Developer Portal", GUILayout.Width(125)))
                {
                    Application.OpenURL("https://dev.epicgames.com/portal");
                }
                if (GUILayout.Button("Docs", GUILayout.Width(50)))
                {
                    Application.OpenURL("https://dev.epicgames.com/docs");
                }
            }
            GUILayout.Label("NOTE: There is no support in the BPT for assigning labels or publishing so that will need to be done manually.");
            
            using (new EditorGUILayout.HorizontalScope())
            {   
                Color temp = GUI.color;
                GUI.color = Utils.PathExists(EpicGames.SDKPath) ? Color.green : Color.red;
                GUILayout.Label(new GUIContent("BuildPatch Path:", "The path to the EpicGamesSDK folder. Build Uploader uses this to upload files to EpicGames."), GUILayout.Width(105));
                GUI.color = temp;
                
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://dev.epicgames.com/docs/epic-games-store/publishing-tools/uploading-binaries/bpt-instructions-170#download-the-buildpatch-tool");
                }

                string path = EpicGames.SDKPath;
                if (CustomFolderPathTextField.OnGUI("EpicGamesSDK Folder", ref path, ref showFormattedSDKPath, ctx))
                {
                    EpicGames.SDKPath = path;
                }
                
                if (GUILayout.Button("CMD", GUILayout.Width(50)))
                {
                    EpicGames.ShowConsole();
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {   
                GUILayout.Label(new GUIContent("Cloud Directory:", "Directory where BuildPatchTool can save files to be uploaded, this can be empty each run. As with the BuildRoot, this can be an absolute or a relative path. (This location is used to cache information about existing binaries, and should be a different directory from the BuildRoot parameter. It is OK if this directory is initially empty; BuildPatchTool will download information as needed from the Epic backend and store it in the CloudDir.)"), GUILayout.Width(105));
                
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://dev.epicgames.com/docs/epic-games-store/publishing-tools/uploading-binaries/bpt-instructions-170#how-to-upload-a-binary");
                }

                string path = EpicGames.CloudPath;
                if (CustomFolderPathTextField.OnGUI("Cloud Directory", ref path, ref showFormattedCloudDirectory, ctx))
                {
                    EpicGames.CloudPath = path;
                }
            }
        }
    }
}
