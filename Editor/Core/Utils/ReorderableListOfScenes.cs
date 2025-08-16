using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfScenes : InternalReorderableList<string>
    {
        private static string[] _allSceneNames;
        private static string[] _allScenePaths;

        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_allSceneNames == null || _allScenePaths == null)
            {
                _allSceneNames = SceneUIUtils.GetScenes().Select(s => s.DisplayName).ToArray();
                _allScenePaths = SceneUIUtils.GetScenes().Select(s => s.Path).ToArray();
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                string element = list[index];

                float width = Mathf.Max(100, rect.width - 100);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                
                EditorGUI.BeginChangeCheck();
                int selectedIndex = Array.IndexOf(_allScenePaths, element);
                int chosen = EditorGUI.Popup(rect1, selectedIndex, _allSceneNames);
                if (EditorGUI.EndChangeCheck())
                {
                    list[index] = _allScenePaths[chosen];
                    dirty = true;
                }
            }
        }

        protected override string CreateItem(int index)
        {
            return "";
        }
    }
}