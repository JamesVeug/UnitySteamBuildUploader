using UnityEngine;

namespace Wireframe
{
    internal partial class SteamSDK
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("steamworks_version", 0);
            set => ProjectEditorPrefs.SetInt("steamworks_version", value);
        }
        
        static SteamSDK()
        {
            int version = ServiceVersion;
            switch (version)
            {
                case 0:
                    // V2.1 - Migrate old preferences to new encoded values
                    EncodedEditorPrefs.MigrateKeyToEncoded<string>("steambuild_SDKUser", UserNameKey);
                    EncodedEditorPrefs.MigrateKeyToEncoded<string>("steambuild_SDKPass", UserPasswordKey);
                    
                    // V3.0 - Migrate to ProjectEditorPrefs
                    ProjectEditorPrefs.MigrateFromEditorPrefs("steambuild_Enabled", ProjectEditorPrefs.PrefType.Bool);
                    ProjectEditorPrefs.MigrateFromEditorPrefs("steambuild_SDKPath", ProjectEditorPrefs.PrefType.String);
                    // Using ProjectID now instead of product name
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "SteamUBuildUploader", UserNameKey);
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "SteamPBuildUploader", UserPasswordKey);
                    break;
            }
            ServiceVersion = CurrentServiceVersion;
        }
    }
}