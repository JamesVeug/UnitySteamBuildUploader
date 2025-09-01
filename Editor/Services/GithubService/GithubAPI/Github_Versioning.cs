using UnityEngine;

namespace Wireframe
{
    internal static partial class Github
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("github_version", 0);
            set => ProjectEditorPrefs.SetInt("github_version", value);
        }

        static Github()
        {
            switch (ServiceVersion)
            {
                case 0:
                    // V0 used Application.productName but that can change when making builds
                    // So we use ProjectEditorPrefs.GetProjectID now.
                    EncodedEditorPrefs.MigrateStringKeyToKey(Application.productName+"GithubTBuildUploader", TokenKey);
                    
                    // Migrated so its project based 
                    ProjectEditorPrefs.MigrateFromEditorPrefs("github_enabled", ProjectEditorPrefs.PrefType.Bool);
                    break;
            }
            ServiceVersion = CurrentServiceVersion;
        }
    }
}