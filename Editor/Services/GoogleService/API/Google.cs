namespace Wireframe
{
    /// <summary>
    /// Shared Google service flag. The Google service umbrella covers both the
    /// Google Drive upload destination and the Google Chat action - both are gated
    /// on the same Enabled toggle.
    /// </summary>
    internal static partial class Google
    {
        public static bool Enabled
        {
            get => ProjectEditorPrefs.GetBool("google_enabled", false);
            set => ProjectEditorPrefs.SetBool("google_enabled", value);
        }
    }
}
