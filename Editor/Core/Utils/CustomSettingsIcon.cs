using UnityEngine;
using Wireframe;

public static class CustomSettingsIcon
{
    public static bool OnGUI()
    {
        if (GUILayout.Button(new GUIContent(Utils.SettingsIcon, "Settings"),
                GUILayout.Width(20), GUILayout.Height(20)))
        {
            return true;
        }
        
        return false;
    }
}