using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public static class SceneUIUtils
    {
        public class SceneData : DropdownElement
        {
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

        public static List<SceneData> GetScenes()
        {
            if (data == null)
            {
                LoadFile();
            }
            return data;
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
                    Path = path,
                    DisplayName = fileName
                });
            }
        }

        public static ScenePopup ScenesPopup => m_ScenePopup ?? (m_ScenePopup = new ScenePopup());
        private static ScenePopup m_ScenePopup;
    }
}