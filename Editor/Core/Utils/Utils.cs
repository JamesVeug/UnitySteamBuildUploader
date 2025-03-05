using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    internal static class Utils
    {
        public static readonly string CacheFolder = Application.persistentDataPath + "/BuildUploader/CachedBuilds";
        
        public static Texture2D WindowIcon
        {
            get
            {
                var iconPath = "Packages/com.veugeljame.builduploader/Icon.png";
                Object loadAssetAtPath = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Object));
                return loadAssetAtPath as Texture2D;
            }
        }
    }
}