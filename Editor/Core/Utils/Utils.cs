using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static class Utils
    {
        public static Texture2D WindowIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/Icon.png";
                UnityEngine.Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(UnityEngine.Object));
                return loadAssetAtPath as Texture2D;
            }
        }
    }
}