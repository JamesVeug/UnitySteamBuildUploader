using UnityEngine;
using Wireframe;

public static class CustomFoldoutButton
{
    public static bool OnGUI(bool collapse)
    {
        var icon = collapse ? Utils.FoldoutClosedIcon : Utils.FoldoutOpenIcon;
        int padding = 5;
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            padding = new RectOffset(padding, padding, padding, padding),
            alignment = TextAnchor.MiddleCenter,
        };
        
        if (GUILayout.Button(icon, style, GUILayout.Width(20), GUILayout.Height(20)))
        {
            return true;
        }

        return false;
    }
}