using System;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Reorderable list of email addresses. Used for the CC and BCC fields of
    /// <see cref="EMailSendMailAction"/>.
    /// </summary>
    public class ReorderableListOfEMailRecipients : InternalReorderableList<string>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            string element = list[index] ?? "";
            string newValue = GUI.TextField(rect, element);
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
