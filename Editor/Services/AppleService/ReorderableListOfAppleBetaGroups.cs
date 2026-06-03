using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Editable list of TestFlight beta groups belonging to the currently-selected app.
    /// </summary>
    public class ReorderableListOfAppleBetaGroups : InternalReorderableList<AppleConfig.AppleBetaGroup>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            AppleConfig.AppleBetaGroup element = list[index];

            float width = Mathf.Min(200, rect.width / 2);

            Rect r1 = new Rect(rect.x, rect.y, width, rect.height);
            string newName = GUI.TextField(r1, element.Name);
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }

            r1.x += r1.width + 10;
            r1.width = rect.x + rect.width - r1.x;
            string newId = GUI.TextField(r1, element.BetaGroupID);
            if (newId != element.BetaGroupID)
            {
                element.BetaGroupID = newId;
                dirty = true;
            }
        }

        protected override AppleConfig.AppleBetaGroup CreateItem(int index)
        {
            return new AppleConfig.AppleBetaGroup(index, "MyBetaGroup", "");
        }

        protected override int CompareTo(AppleConfig.AppleBetaGroup a, AppleConfig.AppleBetaGroup b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
