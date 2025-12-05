using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public partial class EpicGamesUploadDestination
    {
        private bool showFormattedCloudDirectory = Preferences.DefaultShowFormattedTextToggle;
        private bool showFormattedBuildVersion = Preferences.DefaultShowFormattedTextToggle;
        private bool showFormattedAppLaunch = Preferences.DefaultShowFormattedTextToggle;
        private bool showFormattedAppArgs = Preferences.DefaultShowFormattedTextToggle;
        
        protected internal override void OnGUICollapsed(ref bool isDirty, float maxWidth, StringFormatter.Context ctx)
        {
            isDirty |= EpicGamesUIUtils.OrganizationPopup.DrawPopup(ref Organization, ctx);
            isDirty |= EpicGamesUIUtils.ProductPopup.DrawPopup(Organization, ref Product, ctx);
            isDirty |= EpicGamesUIUtils.ArtifactPopup.DrawPopup(Product, ref Artifact, ctx);
        }

        protected internal override void OnGUIExpanded(ref bool isDirty, StringFormatter.Context ctx)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Organization:", "Use the Organization string that was provided along with your credentials."), GUILayout.Width(100));
                isDirty |= EpicGamesUIUtils.OrganizationPopup.DrawPopup(ref Organization, ctx);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Product:", "Use the Product/Game string that was provided along with your credentials."), GUILayout.Width(100));
                isDirty |= EpicGamesUIUtils.ProductPopup.DrawPopup(Organization, ref Product, ctx);
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Artifact:", "Specify the Artifact string that was provided along with your credentials."), GUILayout.Width(100));
                isDirty |= EpicGamesUIUtils.ArtifactPopup.DrawPopup(Product, ref Artifact, ctx);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Build Version:", "The version string for the build. This needs to be unique for each build of a specific artifact, independent of platform. For example, BuildVersion-1.0 can only exists for Windows or Mac, not both. The build version string has the following restrictions: Must be between 1 and 100 chars in length, whitespace is not allowed, should only contain characters from the following sets a-z, A-Z, 0-9, or .+-_"), GUILayout.Width(100));
                isDirty |= EditorUtils.FormatStringTextField(ref BuildVersion, ref showFormattedBuildVersion, ctx);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("App Launch:", "The path to the app executable that should be launched when running your game, relative to (and inside of) the BuildRoot. For Mac binaries, this should be the executable file contained within the .app folder, usually in the location Game.app/Contents/MacOS/Game."), GUILayout.Width(100));
                isDirty |= EditorUtils.FormatStringTextField(ref AppLaunch, ref showFormattedAppLaunch, ctx);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("App Args:", "Optional: The commandline to send to the app on launch."), GUILayout.Width(100));
                isDirty |= EditorUtils.FormatStringTextField(ref AppArgs, ref showFormattedAppArgs, ctx);
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {   
                GUILayout.Label(new GUIContent("Cloud Override:", "Optional: Directory where BuildPatchTool can save files to be uploaded, this can be empty each run. As with the BuildRoot, this can be an absolute or a relative path. (This location is used to cache information about existing binaries, and should be a different directory from the BuildRoot parameter. It is OK if this directory is initially empty; BuildPatchTool will download information as needed from the Epic backend and store it in the CloudDir.)"), GUILayout.Width(100));
                
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL("https://dev.epicgames.com/docs/epic-games-store/publishing-tools/uploading-binaries/bpt-instructions-170#how-to-upload-a-binary");
                }

                string path = CloudDirOverride;
                if (CustomFolderPathTextField.OnGUI("Cloud DirectoryOverride", ref path, ref showFormattedCloudDirectory, ctx))
                {
                    CloudDirOverride = path;
                }
            }
        }
    }
}