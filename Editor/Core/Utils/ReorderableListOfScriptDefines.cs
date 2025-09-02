using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfScriptDefines : ReorderableListOfStrings
    {
        protected override GenericMenu ContextMenu(Event evt)
        {
            GenericMenu menu = base.ContextMenu(evt);
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Add defines from Build Settings"), false, () =>
            {
                List<string> defines = BuildUtils.GetDefaultScriptingDefines();
                foreach (var define in defines)
                {
                    string d = define.Trim();
                    if (!string.IsNullOrEmpty(d) && !list.Contains(d))
                    {
                        list.Add(d);
                        dirty = true;
                    }
                }
            });


            return menu;
        }
    }
}