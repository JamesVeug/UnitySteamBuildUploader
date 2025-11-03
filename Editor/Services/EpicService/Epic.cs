using UnityEditor;

namespace Wireframe
{
    internal static partial class Epic
    {
        private const string EPIC_ENABLED = "epic_enabled";

        private const string EPIC_SDK_PATH = "EpicSDKPath";

        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool(EPIC_ENABLED, false);
            set => ProjectEditorPrefs.SetBool(EPIC_ENABLED, value);
        }

        public static string SDKPath
        {
            get => EditorPrefs.GetString(TokenKey);
            set => EditorPrefs.SetString(TokenKey, value);
        }

        private static string TokenKey => ProjectEditorPrefs.ProjectID + EPIC_SDK_PATH;
    }
}