using UnityEditor;

public class LastBuildDirectoryUtil
{
    public static string LastBuildDirectory
    {
        get => EditorPrefs.GetString("LastBuildDirectory", string.Empty);
        set => EditorPrefs.SetString("LastBuildDirectory", value);
    }
}