using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Wireframe
{
    public static class SceneUIUtils
    {
        public class SceneData : DropdownElement
        {
            public string GUID { get; set; }
            public string Path { get; set; }
            public string DisplayName { get; set; }
            public int Id { get; set; }
        }
        
        public class ScenePopup : CustomDropdown<SceneData>
        {
            public override string FirstEntryText => "Choose Scene";

            protected override List<SceneData> FetchAllData()
            {
                GetScenes();
                return data;
            }
        }

        private static List<SceneData> data = null;
        private static string[] sceneNames = null;
        private static string[] scenePaths = null;
        private static string[] sceneGUIDS = null;
        

        public static void ReloadScenes()
        {
            LoadFile();
        }
        
        public static List<SceneData> GetScenes()
        {
            if (data == null)
            {
                LoadFile();
            }
            return data;
        }
        
        public static string[] GetSceneGUIDS()
        {
            if (data == null)
            {
                GetScenes();
            }
            return sceneGUIDS;
        }
        
        public static string[] GetSceneNames()
        {
            if (data == null)
            {
                GetScenes();
            }
            return sceneNames;
        }
        
        public static string[] GetScenePaths()
        {
            if (data == null)
            {
                GetScenes();
            }
            return scenePaths;
        }

        private static void LoadFile()
        {
            data = new List<SceneData>();
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(path);
                data.Add(new SceneData
                {
                    Id = data.Count + 1,
                    GUID = guid,
                    Path = path,
                    DisplayName = fileName
                });
            }
            
            sceneNames = new string[data.Count];
            scenePaths = new string[data.Count];
            sceneGUIDS = new string[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                sceneNames[i] = data[i].DisplayName;
                scenePaths[i] = data[i].Path;
                sceneGUIDS[i] = data[i].GUID;
            }

            ScenesPopup.Refresh();
        }

        public static ScenePopup ScenesPopup => m_ScenePopup ?? (m_ScenePopup = new ScenePopup());
        private static ScenePopup m_ScenePopup;
    }
}