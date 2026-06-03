namespace Wireframe
{
    public partial class NintendoSDK
    {
        private const int CurrentServiceVersion = 1;

        private static int ServiceVersion
        {
            get => ProjectEditorPrefs.GetInt("nintendo_version", 0);
            set => ProjectEditorPrefs.SetInt("nintendo_version", value);
        }

        static NintendoSDK()
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
