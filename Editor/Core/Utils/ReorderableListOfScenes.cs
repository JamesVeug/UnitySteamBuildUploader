using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfScenes : InternalReorderableList<string>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            string[] allSceneGUIDs = SceneUIUtils.GetSceneGUIDS();
            string[] allScenePaths = SceneUIUtils.GetScenePaths();
            List<string> filteredScenes = new List<string>();
            List<string> filteredScenePaths = new List<string>();
            for (var i = 0; i < allSceneGUIDs.Length; i++)
            {
                string sceneGUID = allSceneGUIDs[i];
                if (!list.Contains(sceneGUID) || list[index] == sceneGUID)
                {
                    filteredScenes.Add(sceneGUID);
                    filteredScenePaths.Add(allScenePaths[i]);
                }
            }
            
            string[] filteredSceneGUIDs = filteredScenes.ToArray();
            string[] filteredScenePathsArray = filteredScenePaths.ToArray();
            
            

            using (new EditorGUILayout.HorizontalScope())
            {
                string element = list[index];

                float width = Mathf.Max(100, rect.width - 100);
                Rect rect1 = new Rect(rect.x, rect.y, width, rect.height);
                
                EditorGUI.BeginChangeCheck();
                int selectedIndex = Array.IndexOf(filteredSceneGUIDs, element);
                int chosen = EditorGUI.Popup(rect1, selectedIndex, filteredScenePathsArray);
                if (EditorGUI.EndChangeCheck())
                {
                    string sceneGUID = filteredSceneGUIDs[chosen];
                    if (!list.Contains(sceneGUID))
                    {
                        list[index] = sceneGUID;
                        dirty = true;
                    }
                }
            }
        }

        protected override GenericMenu ContextMenu(Event evt)
        {
            GenericMenu menu = base.ContextMenu(evt);
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Add scenes in Build settings"), false, () =>
            {
                var buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.guid.ToString());
                foreach (string sceneGUID in buildScenes)
                {
                    if (!list.Contains(sceneGUID))
                    {
                        list.Add(sceneGUID);
                        dirty = true;
                    }
                }
            });
            
            menu.AddItem(new GUIContent("Add missing scenes"), false, () =>
            {
                var allScenes = SceneUIUtils.GetScenes().Select(s => s.GUID);
                foreach (string scene in allScenes)
                {
                    if (!list.Contains(scene))
                    {
                        list.Add(scene);
                        dirty = true;
                    }
                }
            });
            
            menu.AddItem(new GUIContent("Remove invalid scenes"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Remove invalid scenes",
                        "Are you sure you want to remove invalid scenes?",
                        "Yes", "No"))
                {

                    string[] allSceneGUIDs = SceneUIUtils.GetSceneGUIDS();
                    List<string> guidsToRemove = new List<string>();
                    foreach (string addedSceneGUID in list)
                    {
                        if (!allSceneGUIDs.Contains(addedSceneGUID))
                        {
                            guidsToRemove.Add(addedSceneGUID);
                        }
                    }

                    if (guidsToRemove.Count > 0)
                    {
                        foreach (string sceneGUID in guidsToRemove)
                        {
                            list.Remove(sceneGUID);
                        }

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
            string[] allSceneGUIDs = SceneUIUtils.GetSceneGUIDS();
            foreach (string guiD in allSceneGUIDs)
            {
                if (!list.Contains(guiD))
                {
                    return guiD;
                }
            }
            
            return "";
        }
    }
}