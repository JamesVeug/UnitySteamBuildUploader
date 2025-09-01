namespace Wireframe
{
    internal partial class Discord
    {
        private const int CurrentServiceVersion = 1;
        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("discord_version", 0);
            set => ProjectEditorPrefs.SetInt("discord_version", value);
        }
        
        static Discord()
        {
            switch (ServiceVersion)
            {
                case 0:
                    // Moved to project prefs
                    ProjectEditorPrefs.MigrateFromEditorPrefs("discord_enabled", ProjectEditorPrefs.PrefType.Bool);
                    break;
            }

            ServiceVersion = CurrentServiceVersion;
        }
    }
}