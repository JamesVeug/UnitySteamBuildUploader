using System;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    /// <summary>
    /// Reorderable list of file paths to attach to an outgoing email. Each row is a
    /// text field plus a Browse button. Paths are passed through the action's
    /// <see cref="Context"/> so tokens like <c>{sourceFile}</c> resolve at send time.
    /// </summary>
    public class ReorderableListOfEmailAttachments : InternalReorderableList<string>
    {
        private const float BrowseButtonWidth = 70f;
        private const float Padding = 4f;

        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            string element = list[index] ?? "";

            const float labelWidth = 40f;
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            GUI.Label(labelRect, new GUIContent("File", "Path to a file to attach."));

            Rect textRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth - BrowseButtonWidth - Padding, rect.height);
            Rect buttonRect = new Rect(textRect.xMax + Padding, rect.y, BrowseButtonWidth, rect.height);

            string newValue = EditorUtils.PlaceholderTextField(textRect, element, "e.g. {sourceFile} or C:/Builds/game.zip");
            if (newValue != element)
            {
                list[index] = newValue;
                dirty = true;
            }

            if (GUI.Button(buttonRect, "Browse..."))
            {
                string startDirectory = !string.IsNullOrEmpty(element) ? element : Application.dataPath;
                string selected = EditorUtility.OpenFilePanel("Select Attachment", startDirectory, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    list[index] = selected;
                    dirty = true;
                }
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
