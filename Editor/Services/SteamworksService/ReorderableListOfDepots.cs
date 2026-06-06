using System;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfDepots : InternalReorderableList<SteamDepot>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SteamDepot element = list[index];

            float labelWidth = 50;
            float fieldWidth = Mathf.Min(100, rect.width / 2);
            Rect rect1 = new Rect(rect.x, rect.y, labelWidth, rect.height);
            GUI.Label(rect1, new GUIContent("Name", "Display name for this depot. UI only — not sent to Steam."));
            rect1.x += rect1.width;
            rect1.width = fieldWidth;
            string n = EditorUtils.PlaceholderTextField(rect1, element.Name, "e.g. Windows Content");
            if (n != element.Name)
            {
                element.Name = n;
                dirty = true;
            }

            rect1.x += rect1.width;
            rect1.width = 70;
            GUI.Label(rect1, new GUIContent("Depot ID", "Numeric Steam depot ID from your app's depot configuration in Steamworks."));
            rect1.x += rect1.width;
            rect1.width = fieldWidth;
            string textField = EditorUtils.PlaceholderTextField(rect1, element.Depot.DepotID.ToString(), "e.g. 1234561");
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