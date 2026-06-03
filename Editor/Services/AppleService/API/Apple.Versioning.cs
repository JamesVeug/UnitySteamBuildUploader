namespace Wireframe
{
    public static partial class Apple
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("apple_version", 0);
            set => ProjectEditorPrefs.SetInt("apple_version", value);
        }

        static Apple()
        {
            switch (ServiceVersion)
            {
                case 0:
                    // First version — nothing to migrate. Reserved hook for future
                    // changes to EditorPrefs / config key names.
                    break;
            }

            ServiceVersion = CurrentServiceVersion;
        }
    }
}
