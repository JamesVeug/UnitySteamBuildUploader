namespace Wireframe
{
    public partial class PlayStationSDK
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("playstation_version", 0);
            set => ProjectEditorPrefs.SetInt("playstation_version", value);
        }

        static PlayStationSDK()
        {
            int version = ServiceVersion;
            if (version <= 0)
            {
                // Initial version - no migrations needed.
            }

            ServiceVersion = CurrentServiceVersion;
        }
    }
}
