using UnityEngine;

namespace Wireframe
{
    public partial class SteamSDK
    {
        private const int CurrentServiceVersion = 2;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("steamworks_version", 0);
            set => ProjectEditorPrefs.SetInt("steamworks_version", value);
        }
        
        static SteamSDK()
        {
            int version = ServiceVersion;
            if (version <= 0)
            {
                // V2.1 - Migrate old preferences to new encoded values
                EncodedEditorPrefs.MigrateKeyToEncoded<string>("steambuild_SDKUser", UserNameKey);
                EncodedEditorPrefs.DeleteKey("steambuild_SDKPass");

                // V3.0 - Migrate to ProjectEditorPrefs
                ProjectEditorPrefs.MigrateFromEditorPrefs("steambuild_Enabled", ProjectEditorPrefs.PrefType.Bool);
                ProjectEditorPrefs.MigrateFromEditorPrefs("steambuild_SDKPath", ProjectEditorPrefs.PrefType.String);
                // Using ProjectID now instead of product name
                EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "SteamUBuildUploader", UserNameKey);
                EncodedEditorPrefs.DeleteKey(Application.productName + "SteamPBuildUploader");
            }
            if (version <= 1)
            {
                // v3.0a - Removal of steam password since we no longer login to authorize the server
                EncodedEditorPrefs.DeleteKey("steambuild_SDKPass");
                EncodedEditorPrefs.DeleteKey(Application.productName + "SteamPBuildUploader");
            }
            
            ServiceVersion = CurrentServiceVersion;
        }
    }
}