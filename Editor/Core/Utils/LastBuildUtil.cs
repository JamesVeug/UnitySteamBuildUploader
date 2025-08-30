using UnityEditor;

public class LastBuildUtil
{
    public static string LastBuildDirectory
    {
        get => EditorPrefs.GetString("LastBuildDirectory", string.Empty);
        private set => EditorPrefs.SetString("LastBuildDirectory", value);
    }
    
    public static string LastBuildName
    {
        get => EditorPrefs.GetString("LastBuildName", string.Empty);
        private set => EditorPrefs.SetString("LastBuildName", value);
    }
    
    public static void SetLastBuild(string path, string buildName)
    {
        LastBuildDirectory = path;
        LastBuildName = buildName;
    }
}