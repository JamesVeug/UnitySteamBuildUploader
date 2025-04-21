using UnityEngine;

namespace Wireframe
{
    public static class CustomTextField
    {
        public static bool Draw(ref string text, params GUILayoutOption[] options)
        {
            string newPath = GUILayout.TextField(text, options);
            if (text != newPath)
            {
                text = newPath;
                return true;
            }

            return false;
        }
    }
}