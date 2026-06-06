using System;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Reorderable list of email addresses. Used for the CC and BCC fields of
    /// <see cref="EmailSendMailAction"/>.
    /// </summary>
    public class ReorderableListOfEmailRecipients : InternalReorderableList<string>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            string element = list[index] ?? "";

            const float labelWidth = 50f;
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            GUI.Label(labelRect, new GUIContent("Email", "Recipient email address for this CC/BCC entry."));
            Rect fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);
            string newValue = EditorUtils.PlaceholderTextField(fieldRect, element, "e.g. teammate@studio.com");
            if (newValue != element)
            {
                list[index] = newValue;
                dirty = true;
            }
        }

        protected override string CreateItem(int index)
        {
            return "";
        }

        protected override int CompareTo(string a, string b)
        {
            return String.Compare(a, b, StringComparison.Ordinal);
        }
    }
}
