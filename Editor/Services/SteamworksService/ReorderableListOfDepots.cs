using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Wireframe;

namespace Wireframe
{
    public class ReorderableListOfDepots : InternalReorderableList<SteamDepot>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SteamDepot element = list[index];

            Rect rect1 = new Rect(rect.x, rect.y, Mathf.Min(100, rect.width / 2), rect.height);
            string n = GUI.TextField(rect1, element.Name);
            if (n != element.Name)
            {
                element.Name = n;
                dirty = true;
            }

            rect1.x += rect1.width;
            string textField = GUI.TextField(rect1, element.Depot.DepotID.ToString());
            if (int.TryParse(textField, out int value) && value != element.Depot.DepotID)
            {
                element.Depot.DepotID = value;
                dirty = true;
            }
        }

        protected override SteamDepot CreateItem(int index)
        {
            return new SteamDepot(index, "");
        }

        protected override int CompareTo(SteamDepot a, SteamDepot b)
        {
            return String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
        }
    }
}