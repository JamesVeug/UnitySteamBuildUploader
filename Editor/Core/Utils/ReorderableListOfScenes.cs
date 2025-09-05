using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfScenes : InternalReorderableList<string>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            string[] _allScenePaths = SceneUIUtils.GetScenePaths();
            string[] _allSceneNames = SceneUIUtils.GetSceneNames();

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

        protected override GenericMenu ContextMenu(Event evt)
        {
            GenericMenu menu = base.ContextMenu(evt);
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Add scenes in Build settings"), false, () =>
            {
                var buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path);
                foreach (var scene in buildScenes)
                {
                    if (!list.Contains(scene))
                    {
                        list.Add(scene);
                        dirty = true;
                    }
                }
            });
            
            menu.AddItem(new GUIContent("Add missing scenes"), false, () =>
            {
                var allScenes = SceneUIUtils.GetScenes().Select(s => s.Path);
                foreach (var scene in allScenes)
                {
                    if (!list.Contains(scene))
                    {
                        list.Add(scene);
                        dirty = true;
                    }
                }
            });
            
            return menu;
        }

        protected override bool DrawHeader()
        {
            bool openFolderOut = base.DrawHeader();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh Scenes", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                SceneUIUtils.ReloadScenes();
            }


            return openFolderOut;
        }

        protected override string CreateItem(int index)
        {
            return "";
        }
    }
}