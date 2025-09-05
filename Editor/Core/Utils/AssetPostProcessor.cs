using UnityEditor;

namespace Wireframe
{
    public class AssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool scenesChanged = false;
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".unity"))
                {
                    scenesChanged = true;
                    break;
                }
            }
            
            if (!scenesChanged)
            {
                for (int i = 0; i < deletedAssets.Length; i++)
                {
                    if (deletedAssets[i].EndsWith(".unity"))
                    {
                        scenesChanged = true;
                    }
                }
            }

            if (!scenesChanged)
            {
                for (int i = 0; i < movedAssets.Length; i++)
                {
                    if (movedAssets[i].EndsWith(".unity"))
                    {
                        scenesChanged = true;
                    }
                }
            }

            if (scenesChanged)
            {
                SceneUIUtils.ReloadScenes();
            }
        }
    }
}