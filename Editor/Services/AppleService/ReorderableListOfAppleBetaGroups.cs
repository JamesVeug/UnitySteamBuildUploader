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

            float labelWidth = 50f;
            float padding = 5f;

            // Name — friendly label shown in popups; UI only.
            Rect r = new Rect(rect.x, rect.y, labelWidth, rect.height);
            GUI.Label(r, new GUIContent("Name", "Display name for this beta group (e.g. External Testers). UI only — not sent to Apple."));
            r.x += r.width;
            r.width = Mathf.Min(150, (rect.x + rect.width - r.x) / 2);
            string newName = EditorUtils.PlaceholderTextField(r, element.Name, "e.g. External Testers");
            if (newName != element.Name)
            {
                element.Name = newName;
                dirty = true;
            }
            r.x += r.width + padding;

            // Beta Group ID — App Store Connect "betaGroup" resource ID (a UUID).
            r.width = labelWidth + 10;
            GUI.Label(r, new GUIContent("Group ID", "App Store Connect \"betaGroup\" resource ID (a UUID). Find it in the App Store Connect URL when viewing the group."));
            r.x += r.width;
            r.width = rect.x + rect.width - r.x;
            string newId = EditorUtils.PlaceholderTextField(r, element.BetaGroupID, "e.g. 12a3b4c5-6789-0abc-def0-1234567890ab");
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
