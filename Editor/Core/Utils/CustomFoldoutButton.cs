using UnityEngine;
using Wireframe;

public static class CustomFoldoutButton
{
    public static bool OnGUI(bool collapse)
    {
        var icon = collapse ? Utils.FoldoutClosedIcon : Utils.FoldoutOpenIcon;
        if (GUILayout.Button(icon, GUILayout.Width(20), GUILayout.Height(20)))
        {
            return true;
        }

        return false;
    }
}