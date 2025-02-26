using UnityEngine;

namespace Wireframe
{
    internal class ReorderableListOfBranches : InternalReorderableList<SteamBranch>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SteamBranch element = list[index];

            Rect rect1 = new Rect(rect.x, rect.y, Mathf.Min(100, rect.width / 2), rect.height);
            string n = GUI.TextField(rect1, element.name);
            if (n != element.name)
            {
                element.name = n;
                dirty = true;
            }
        }

        protected override SteamBranch CreateItem(int index)
        {
            return new SteamBranch(index, "");
        }
    }
}