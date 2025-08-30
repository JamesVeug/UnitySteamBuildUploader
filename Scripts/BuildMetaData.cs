using System;
using System.IO;
using UnityEngine;

namespace Wireframe
{
    [Serializable]
    public class BuildMetaData
    {
        public int BuildNumber;

        public static BuildMetaData Get()
        {
#if UNITY_EDITOR
            BuildMetaData data = new BuildMetaData();
            data.BuildNumber = 123456789;
            return data;
#else
            
            // Get from build settings
            string streamingAssetsPath = Application.streamingAssetsPath;
            string path = Path.Combine(streamingAssetsPath, "BuildData") + ".json";
            if(File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<BuildMetaData>(json);
            }

            CloudBuildManifest cloudBuildManifest = CloudBuildManifest.Instance;
            if (cloudBuildManifest != null)
            {
                BuildMetaData data = new BuildMetaData();
                data.BuildNumber = cloudBuildManifest.BuildNumber;
                return data;
            }
            
            return new BuildMetaData(); // Not build using build uploader and or cloud build
#endif
        }
    }
}