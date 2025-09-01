using UnityEngine;

namespace Wireframe
{
    internal partial class UnityCloud
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("unitycloud_version", 0);
            set => ProjectEditorPrefs.SetInt("unitycloud_version", value);
        }
        
        static UnityCloud()
        {
            int version = ServiceVersion;
            switch (version)
            {
                case 0:
                    // v2.1.0 Migrate everything over to encoded values
                    EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_organization", OrganizationKey);
                    EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_project", ProjectKey);
                    EncodedEditorPrefs.MigrateKeyToEncoded<string>("unityCloud_secret", SecretKey);
                    
                    // v3.0.0 Migrate to project based prefs
                    ProjectEditorPrefs.MigrateFromEditorPrefs("unityCloud_enabled", ProjectEditorPrefs.PrefType.Bool);
                    // Using ProjectID now instead of product name
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "UnityCloudOBuildUploader", OrganizationKey);
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "UnityCloudPBuildUploader", ProjectKey);
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName + "UnityCloudSBuildUploader", SecretKey);
                    break;
            }
            ServiceVersion = CurrentServiceVersion;
        }
    }
}