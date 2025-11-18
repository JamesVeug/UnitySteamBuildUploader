using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal partial class EpicGamesService
    {
        private StringFormatter.Context ctx = new StringFormatter.Context();
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
                GUILayout.Label(new GUIContent("BuildPatch Path:", "The path to the EpicGamesSDK folder. Build Uploader uses this to upload files to EpicGames."), GUILayout.Width(105));
                
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://dev.epicgames.com/docs/epic-games-store/publishing-tools/uploading-binaries/bpt-instructions-170#download-the-buildpatch-tool");
                }

                string path = EpicGames.SDKPath;
                if (CustomFolderPathTextField.OnGUI("EpicGamesSDK Folder", ref path, ref showFormattedSDKPath, ctx))
                {
                    EpicGames.SDKPath = path;
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
