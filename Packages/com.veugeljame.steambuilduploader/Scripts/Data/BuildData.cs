using UnityEngine;
using UnityEditor;

namespace Wireframe
{
    public class BuildData : ScriptableObject
    {
        public int BuildNumber;

        public void SaveAsset()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}